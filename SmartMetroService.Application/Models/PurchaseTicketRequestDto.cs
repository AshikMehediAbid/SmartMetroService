namespace SmartMetroService.Application.Models;

public class PurchaseTicketRequestDto
{
    public int? FromStationId { get; set; }
    public int? ToStationId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentFor? PaymentFor { get; set; }
}

public enum PaymentMethod
{
    Online = 1,
    AccountBalance = 2
}

public enum PaymentFor
{
    SingleJourney = 1,
    RapidPass = 2,
    WalletRecharge = 3
}