namespace SmartMetroService.Application.Models;

public class PurchaseTicketRequestDto
{
    public int FromStationId { get; set; }
    public int ToStationId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
}

public enum PaymentMethod
{
    Online = 1,
    AccountBalance = 2
}
