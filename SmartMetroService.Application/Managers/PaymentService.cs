using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Managers;

public class PaymentService : IPaymentService
{
    private readonly IStationService _stationService;
    private readonly IWalletService _walletService;

    public PaymentService(IStationService stationService, IWalletService walletService)
    {
        _stationService = stationService;
        _walletService = walletService;
    }
    public async Task CompletePaymentAsync(PurchaseTicketRequestDto purchaseTicketRequestDto)
    {
        if(purchaseTicketRequestDto.PaymentMethod == PaymentMethod.AccountBalance)
        {
            await HandleAccountPayment(purchaseTicketRequestDto);
        }
    }

    private async Task HandleAccountPayment(PurchaseTicketRequestDto request)
    {
        var journeyFare = await _stationService.GetFare(request.FromStationId, request.ToStationId);
        var userBalance = await _walletService.GetBalanceByEmailAsync(request.UserEmail);

        if (journeyFare[0].Fare > userBalance)
        {
            throw new Exception("Insufficient Balance");
        }

        // Reduce Account Balance
        await _walletService.DecreaseAccountBalanceAsync(request.UserEmail, journeyFare[0].Fare);
    }
}
