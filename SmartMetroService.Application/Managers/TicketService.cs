using AutoMapper;
using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

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

        var ticketEntity = new Ticket()
        {
            UserId = user.Id,
            FromStationId = request.FromStationId,
            ToStationId = request.ToStationId,
            Fare = request.Fare,
            ExpiryTime = DateTime.Now.AddDays(2),
            TicketStatus = TicketStatus.Fresh,
            TicketType = request.TicketType
        };


        var ticket = await _unitOfWork.TicketRepository.AddAsync(ticketEntity);
        await _unitOfWork.CompleteAsync();

        ticket.QRByte = GenerateQrByte(ticket.Id);
        await _unitOfWork.CompleteAsync();
    }

    private byte[]? GenerateQrByte(Guid id)
    {
        var encriptedQrData = _encryptionService.Encrypt(id.ToString());

        var qrCode = _qrCodeService.GenerateQrCode(encriptedQrData);

        return qrCode;
    }

    /*    public async Task<List<TicketResponseDto>?> GetTicketsOfAUserByTicketStatus(TicketRequestDto request)
        {
            var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(request.UserEmail);

            if (user is null)
                throw new UnauthorizedException("User not found");

            List<Ticket>? ticketsEntity = await _unitOfWork.TicketRepository.GetTicketsOfAUserByTicketStatusAsync(request);

            var tickets = _mapper.Map<List<TicketResponseDto>>(ticketsEntity);

            return tickets;
        }*/

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

    public async Task<Ticket> GetTicketByIdAsync(Guid ticketId)
    {
        var ticket = await _unitOfWork.TicketRepository.GetTicketByIdAsync(ticketId);

        return ticket;

    }
}
