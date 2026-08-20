using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IPaymentService
{
    Task CompletePaymentAsync(PurchaseTicketRequestDto purchaseTicketRequestDto);
}
