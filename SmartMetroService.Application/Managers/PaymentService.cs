using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class PaymentService : IPaymentService
{
    private readonly IStationService _stationService;
    private readonly IWalletService _walletService;
    private readonly ITicketService _ticketService;
    private readonly IAamarPayService _aamarPayService;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IStationService stationService, 
        IWalletService walletService, 
        ITicketService ticketService, 
        IAamarPayService aamarPayService, 
        IUnitOfWork unitOfWork)
    {
        _stationService = stationService;
        _walletService = walletService;
        _ticketService = ticketService;
        _aamarPayService = aamarPayService;
        _unitOfWork = unitOfWork;
    }
    public async Task<string?> CompletePaymentAsync(PurchaseTicketRequestDto purchaseTicketRequest)
    {
        var paymentFor = purchaseTicketRequest.PaymentFor ?? PaymentFor.SingleJourney;
        if (paymentFor == PaymentFor.SingleJourney)
        {
            var journeyFare = await _stationService.GetFare(purchaseTicketRequest.FromStationId ?? 0, purchaseTicketRequest.ToStationId ?? 0);
            if (journeyFare.Count == 0)
                throw new InvalidOperationException("No fare is available for the selected stations.");

            purchaseTicketRequest.Amount = journeyFare[0].Fare;
        }
        else if (purchaseTicketRequest.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        var fare = purchaseTicketRequest.Amount;
        if (purchaseTicketRequest.PaymentMethod == PaymentMethod.AccountBalance)
        {
            if (paymentFor != PaymentFor.SingleJourney)
                throw new NotSupportedException("Rapid pass and wallet recharge require online payment.");

            await HandleAccountPayment(purchaseTicketRequest, fare);
            await GenerateQrTicket(purchaseTicketRequest, fare, TicketType.SingleJourney);
            return null;
        }

        if (purchaseTicketRequest.PaymentMethod != PaymentMethod.Online)
        {
            throw new NotSupportedException("Unsupported payment method.");
        }

        purchaseTicketRequest.Amount = fare;
        return await _aamarPayService.InitiatePaymentAsync(purchaseTicketRequest);
    }

    public async Task<PaymentStatusResponseDto?> GetPaymentStatusAsync(string merTxnId)
    {
        Payment? payment = await _unitOfWork.PaymentRepository.GetByMerTxnIdAsync(merTxnId);

        if (payment == null)
        {
            return null;
        }

        var paymentStatusResponse = new PaymentStatusResponseDto
        {
            TransactionId = payment.MerTxnId,
            Status = payment.Status,
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentMethod = payment.PaymentProcessor,
            PgTransactionId = payment.PgTxnId,
            BankTransactionId = payment.BankTrxId,
            PaidAt = payment.PaidAt
        };

        return paymentStatusResponse;
    }

    public async Task<AamarPayValidationResponse> ValidatePaymentAsync(string mer_txnid)
    {
        var validationResult = await _aamarPayService.ValidatePaymentAsync(mer_txnid);
        return validationResult;
    }

    public async Task<bool> FinalizePaymentAsync(string merTxnId, AamarPayValidationResponse validationResult)
    {
        var payment = await _unitOfWork.PaymentRepository.GetByMerTxnIdAsync(merTxnId);
        if (payment == null || !string.Equals(payment.MerTxnId, validationResult.MerTxnId ?? merTxnId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(payment.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(validationResult.PayStatus, "Successful", StringComparison.OrdinalIgnoreCase)
            || !decimal.TryParse(validationResult.Amount, out var gatewayAmount)
            || gatewayAmount != payment.Amount)
        {
            return false;
        }

        if (payment.PaymentFor == (int)PaymentFor.WalletRecharge)
        {
            await _walletService.IncreaseAccountBalanceAsync(payment.UserEmail, payment.Amount);
        }
        else
        {
            var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(payment.UserEmail)
                ?? throw new InvalidOperationException("Payment user not found");

            await GenerateQrTicket(new PurchaseTicketRequestDto
            {
                FromStationId = payment.FromStationId,
                ToStationId = payment.ToStationId,
                UserEmail = user.Email
            }, (int)payment.Amount, (TicketType)payment.TicketType);
            payment.TicketGenerated = true;
        }

        payment.Status = "Paid";
        payment.PgTxnId = validationResult.PgTxnId;
        payment.BankTrxId = validationResult.BankTrxId;
        payment.PaymentProcessor = validationResult.PaymentProcessor ?? payment.PaymentProcessor;
        payment.PaidAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync();

        await _unitOfWork.CompleteAsync();

        return true;
    }

    public async Task UpdatePaymentStatusAsync(string merTxnId, string status)
    {
        var payment = await _unitOfWork.PaymentRepository.GetByMerTxnIdAsync(merTxnId);
        if (payment == null || string.Equals(payment.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        payment.Status = status;
        await _unitOfWork.CompleteAsync();
    }

    private async Task GenerateQrTicket(PurchaseTicketRequestDto purchaseTicketRequestDto, int fare, TicketType type)
    {
        var request = new GenerateTicketRequestDto()
        {
            FromStationId = purchaseTicketRequestDto.FromStationId ?? 0,
            ToStationId = purchaseTicketRequestDto.ToStationId ?? 0,
            Fare = fare,
            UserEmail = purchaseTicketRequestDto.UserEmail,
            TicketType = type
        };

        await _ticketService.GenerateQrTicketAsync(request);
    }

    private async Task<int> HandleAccountPayment(PurchaseTicketRequestDto request, int fare)
    {
        var userBalance = await _walletService.GetBalanceByEmailAsync(request.UserEmail);

        if (fare > userBalance)
        {
            throw new Exception("Insufficient Balance");
        }

        // Reduce Account Balance
        await _walletService.DecreaseAccountBalanceAsync(request.UserEmail, fare);

        return fare;
    }
}
