using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class ScannerService : IScannerService
{
    private readonly IEncryptionService _encryptionService;
    private readonly ITicketService _ticketService;

    private readonly IUnitOfWork _unitOfWork;

    public ScannerService(IEncryptionService encryptionService, ITicketService ticketService, IUnitOfWork unitOfWork)
    {
        _encryptionService = encryptionService;
        _ticketService = ticketService;
        _unitOfWork = unitOfWork;
    }

    public async Task ValidateQrData(QrCodeRequest qrCodeRequest)
    {
        try
        {
            var ticketId = _encryptionService.Decrypt(qrCodeRequest.QrCode);

            Ticket ticket = await _ticketService.GetTicketByIdAsync(Guid.Parse(ticketId));

            if (ticket.TicketStatus == TicketStatus.Fresh)
            {
                await HandleFreshTicket(ticket, qrCodeRequest.StationId, qrCodeRequest.Gate);
            }

            else if (ticket.TicketStatus == TicketStatus.InUse)
            {
                await HandleInUseTicket(ticket, qrCodeRequest.StationId, qrCodeRequest.Gate);
            }

            else if (ticket.TicketStatus == TicketStatus.Used)
            {
                throw new Exception("This ticket is already been used");
            }

            else if (ticket.TicketStatus == TicketStatus.Expired)
            {
                throw new Exception("This ticket is already expire. Use A new ticket");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }


    }


    private async Task HandleFreshTicket(Ticket ticket, int stationId, string gate)
    {
        if (ticket.ExpiryTime < DateTime.Now)
        {
            ticket.ExpiryTime = DateTime.Now;
            await _unitOfWork.CompleteAsync();

            throw new Exception("This ticket is already expire. Use A new ticket");
        }

        if (gate == "Exit")
        {
            throw new Exception("Scan your Qr ticket in the Entry Gate. Thank you");
        }

        var currentStation = await _unitOfWork.StationRepository.GetStationByIdAsync(stationId);

        var fromStation = await _unitOfWork.StationRepository.GetStationByIdAsync(ticket.FromStationId ?? 0);
        var toStation = await _unitOfWork.StationRepository.GetStationByIdAsync(ticket.ToStationId ?? 0);


        if (currentStation?.StationOrder < Math.Min(fromStation.StationOrder,toStation.StationOrder) ||
            currentStation?.StationOrder > Math.Max(fromStation.StationOrder, toStation.StationOrder))
        {
            throw new Exception($"Wrong Entry!! Your Starting point is {ticket.FromStation}");
        }

        ticket.TicketStatus = TicketStatus.InUse;
        ticket.UpdatedAt = DateTime.Now;
        await _unitOfWork.CompleteAsync();

    }

    private async Task HandleInUseTicket(Ticket ticket, int stationId, string gate)
    {
        if (ticket.ExpiryTime < DateTime.Now)
        {
            ticket.ExpiryTime = DateTime.Now;
            await _unitOfWork.CompleteAsync();

            throw new Exception("This ticket has already expired. Please contact the help desk.");
        }

        if (gate == "Entry")
        {
            throw new Exception("Scan your Qr ticket in the Exit Gate. Thank you");
        }

        var currentStation = await _unitOfWork.StationRepository.GetStationByIdAsync(stationId);

        var fromStation = await _unitOfWork.StationRepository.GetStationByIdAsync(ticket.FromStationId ?? 0);
        var toStation = await _unitOfWork.StationRepository.GetStationByIdAsync(ticket.ToStationId ?? 0);

        if (currentStation?.StationOrder < Math.Min(fromStation.StationOrder, toStation.StationOrder) ||
            currentStation?.StationOrder > Math.Max(fromStation.StationOrder, toStation.StationOrder))
        {
            throw new Exception($"Wrong Exit!! Your Exit point is {ticket.ToStation}");
        }

        ticket.TicketStatus = TicketStatus.Used;
        ticket.UpdatedAt = DateTime.Now;
        await _unitOfWork.CompleteAsync();

    }
}
