using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IAamarPayService
{
    Task<string> InitiatePaymentAsync(PurchaseTicketRequestDto paymentRequest);
    Task<AamarPayValidationResponse> ValidatePaymentAsync(string mer_txnid);
}
