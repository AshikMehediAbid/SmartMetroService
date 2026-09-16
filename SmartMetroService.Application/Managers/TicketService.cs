using AutoMapper;
using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using System.Text.Json;

namespace SmartMetroService.Application.Managers;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEncryptionService _encryptionService;
    private readonly IQrCodeService _qrCodeService;

    public TicketService(IUnitOfWork unitOfWork, IMapper mapper, IEncryptionService encryptionService, IQrCodeService qrCodeService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _encryptionService = encryptionService;
        _qrCodeService = qrCodeService;
    }

    public async Task GenerateQrTicketAsync(GenerateTicketRequestDto request)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(request.UserEmail);

        if (user is null)
        {
            throw new Exception("User not found");
        }
        var expiryTime = DateTime.Now.AddDays(2);
        var ticketEntity = new Ticket()
        {
            UserId = user.Id,
            FromStationId = request.FromStationId,
            ToStationId = request.ToStationId,
            Fare = request.Fare,
            ExpiryTime = expiryTime,
            TicketStatus = TicketStatus.Fresh,
            TicketType = request.TicketType
        };


        var ticket = await _unitOfWork.TicketRepository.AddAsync(ticketEntity);
        await _unitOfWork.CompleteAsync();

        ticket.QRByte = GenerateQrByte(ticket.Id, expiryTime, request.TicketType.ToString());
        await _unitOfWork.CompleteAsync();
    }

    private byte[] GenerateQrByte(Guid id,DateTime expiryTime, string qrType)
    {
        var payload = new QrPayload
        {
            Id = id,
            ExpiryTime = expiryTime,
            QrType = qrType
        };

        var json = JsonSerializer.Serialize(payload);

        var encriptedQrData = _encryptionService.Encrypt(json);

        var qrCode = _qrCodeService.GenerateQrCode(encriptedQrData);

        return qrCode;
    }


    public async Task<List<TicketResponseDto>?> GetTicketsOfAUserByTicketStatus(string? userEmail, TicketStatus ticketStatus)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(userEmail);

        if (user is null)
            throw new UnauthorizedException("User not found");

        await _unitOfWork.TicketRepository.MarkOldFreshTicketsAsExpiredAsync(user.Id);
        await _unitOfWork.CompleteAsync();

        var tickets = await _unitOfWork.TicketRepository.GetTicketsOfAUserByTicketStatusAsync(user.Id, ticketStatus);

        var ticketDto = _mapper.Map<List<TicketResponseDto>>(tickets);

        return ticketDto;
    }

    public async Task<RapidPassResponseDto> GetOrCreateUserRapidPass(string? userEmail)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(userEmail);

        if (user is null)
        {
            throw new Exception("User not found");
        }

        // Get Rapid Pass
        RapidPass? rapidPassTicket = await _unitOfWork.RapidPassRepository.GetByUserIdAsync(user.Id);

        if (rapidPassTicket is not null)
        {
            if (rapidPassTicket.ExpiryTime < DateTime.Now)
            {
                rapidPassTicket.ExpiryTime = DateTime.Now.AddDays(1);

                rapidPassTicket.QRByte = GenerateQrByte(rapidPassTicket.Id, rapidPassTicket.ExpiryTime.Value, "RapidPass");

                await _unitOfWork.CompleteAsync();
            }
            var rapidPassDto = _mapper.Map<RapidPassResponseDto>(rapidPassTicket);
            return rapidPassDto;
        }


        // Create Rapid Pass
        var ticketEntity = new RapidPass()
        {
            UserId = user.Id,
            ExpiryTime = DateTime.Now.AddDays(1),
        };

        rapidPassTicket = await _unitOfWork.RapidPassRepository.AddAsync(ticketEntity);
        await _unitOfWork.CompleteAsync();

        rapidPassTicket.ExpiryTime = DateTime.Now.AddDays(1);
        rapidPassTicket.QRByte = GenerateQrByte(rapidPassTicket.Id, rapidPassTicket.ExpiryTime.Value, "RapidPass");
        await _unitOfWork.CompleteAsync();

        var rapidPassTicketDto = _mapper.Map<RapidPassResponseDto>(rapidPassTicket);

        return rapidPassTicketDto;
    }
}
