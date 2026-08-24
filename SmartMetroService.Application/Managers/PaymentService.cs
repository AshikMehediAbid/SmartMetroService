using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class PaymentService : IPaymentService
{
    private readonly IStationService _stationService;
    private readonly IWalletService _walletService;
    private readonly ITicketService _ticketService;

    public PaymentService(IStationService stationService, IWalletService walletService, ITicketService ticketService)
    {
        _stationService = stationService;
        _walletService = walletService;
        _ticketService = ticketService;
    }
    public async Task CompletePaymentAsync(PurchaseTicketRequestDto purchaseTicketRequestDto)
    {
        try
        {
            if (purchaseTicketRequestDto.PaymentMethod == PaymentMethod.AccountBalance)
            {
                var fare = await HandleAccountPayment(purchaseTicketRequestDto);

                await GenerateQrTicket(purchaseTicketRequestDto, fare, TicketType.SingleJourney);
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    private async Task GenerateQrTicket(PurchaseTicketRequestDto purchaseTicketRequestDto, int fare, TicketType type)
    {
        var request = new GenerateTicketRequestDto()
        {
            FromStationId = purchaseTicketRequestDto.FromStationId,
            ToStationId = purchaseTicketRequestDto.ToStationId,
            Fare = fare,
            UserEmail = purchaseTicketRequestDto.UserEmail,
            TicketType = type
        };

        await _ticketService.GenerateQrTicketAsync(request);
    }

    private async Task<int> HandleAccountPayment(PurchaseTicketRequestDto request)
    {
        var journeyFare = await _stationService.GetFare(request.FromStationId, request.ToStationId);
        var userBalance = await _walletService.GetBalanceByEmailAsync(request.UserEmail);

        if (journeyFare[0].Fare > userBalance)
        {
            throw new Exception("Insufficient Balance");
        }

        // Reduce Account Balance
        await _walletService.DecreaseAccountBalanceAsync(request.UserEmail, journeyFare[0].Fare);

        return journeyFare[0].Fare;
    }
}
