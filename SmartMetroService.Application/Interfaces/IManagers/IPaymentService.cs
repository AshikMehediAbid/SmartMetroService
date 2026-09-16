using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IPaymentService
{
    Task<string?> CompletePaymentAsync(PurchaseTicketRequestDto purchaseTicketRequestDto);
    Task<PaymentStatusResponseDto?> GetPaymentStatusAsync(string merTxnId);
    Task<AamarPayValidationResponse> ValidatePaymentAsync(string mer_txnid);
    Task<bool> FinalizePaymentAsync(string merTxnId, AamarPayValidationResponse validationResult);
    Task UpdatePaymentStatusAsync(string merTxnId, string status);
}
