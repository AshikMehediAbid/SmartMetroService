using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SmartMetroService.Application.Managers;

public class ScannerService : IScannerService
{
    private readonly IEncryptionService _encryptionService;
    private readonly ITicketService _ticketService;
    private readonly IStationService _stationService;
    private readonly IDistributedCache _cache;

    private readonly IUnitOfWork _unitOfWork;

    public ScannerService(
        IEncryptionService encryptionService,
        ITicketService ticketService,
        IStationService stationService,
        IDistributedCache cache,
        IUnitOfWork unitOfWork)
    {
        _encryptionService = encryptionService;
        _ticketService = ticketService;
        _stationService = stationService;
        _cache = cache;
        _unitOfWork = unitOfWork;
    }

    public async Task ValidateQrData(QrCodeRequest qrCodeRequest)
    {
        try
        {
            var ticketData = _encryptionService.Decrypt(qrCodeRequest.QrCode);

            var qrPayload = JsonSerializer.Deserialize<QrPayload>(ticketData);

            if(qrPayload is null)
            {
                throw new Exception("Invalid QR code data");
            }

            if(qrPayload.QrType == TicketType.SingleJourney.ToString())
            {
                await HandleSingleJourneyTicket(qrPayload, qrCodeRequest);
            }
            else
            {
                await HandleRapidPassJourney(qrPayload, qrCodeRequest);
            }

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }


    }

    private async Task HandleRapidPassJourney(QrPayload qrPayload, QrCodeRequest qrCodeRequest)
    {
        var rapidPassId = Guid.Parse(qrPayload.TicketId);
        var rapidPass = await _unitOfWork.RapidPassRepository.GetByIdAsync(rapidPassId);

        if (rapidPass is null)
            throw new Exception("Rapid pass not found");

        var wallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(rapidPass.UserId);

        if (wallet is null)
            throw new Exception("Wallet not found");

        var balance = wallet.Balance;

        var settings = await _unitOfWork.AdminRepository.GetSettingsAsync();

        if (balance < settings.MinimumFare)
            throw new Exception("Insufficient Balance. Please recharge your wallet. Thank You!");

        var cacheKey = rapidPassId.ToString();
        var gate = qrCodeRequest.Gate.Trim();

        if (gate.Equals("Entry", StringComparison.OrdinalIgnoreCase))
        {
            var entryData = new RapidPassJourneyCache
            {
                StationId = qrCodeRequest.StationId,
                EntryTime = DateTime.UtcNow,
                Gate = gate
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(entryData),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });

            return;
        }

        if (!gate.Equals("Exit", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Invalid gate");

        var cachedJourney = await _cache.GetStringAsync(cacheKey);
        if (cachedJourney is null)
            throw new Exception("No active journey found for this rapid pass");

        var entry = JsonSerializer.Deserialize<RapidPassJourneyCache>(cachedJourney);
        if (entry is null || !entry.Gate.Equals("Entry", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Invalid rapid pass journey data");

        var fare = await _stationService.GetFare(entry.StationId, qrCodeRequest.StationId);
        if (fare is null || fare.Count == 0)
            throw new Exception("Unable to calculate journey fare");

        if (fare[0].Fare > balance)
            throw new Exception("Insufficient Balance. Please recharge your wallet. Thank You!");

        var journey = new Journey
        {
            UserId = rapidPass.UserId,
            RapidPassId = rapidPass.Id,
            FromStationId = entry.StationId,
            ToStationId = qrCodeRequest.StationId,
            Fare = fare[0].Fare,
            StartAt = entry.EntryTime,
            EndAt = DateTime.UtcNow,
            JourneyStatus = JourneyStatus.Complete
        };

        await _unitOfWork.JourneyRepository.AddAsync(journey);
        wallet.Balance -= journey.Fare;
        await _unitOfWork.CompleteAsync();

        await _cache.RemoveAsync(cacheKey);
    }

    private sealed class RapidPassJourneyCache
    {
        public int StationId { get; set; }
        public DateTime EntryTime { get; set; }
        public string Gate { get; set; } = string.Empty;
    }

    private async Task HandleSingleJourneyTicket(QrPayload qrPayload, QrCodeRequest qrCodeRequest)
    {
        try
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(Guid.Parse(qrPayload.TicketId));

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
