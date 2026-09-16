namespace SmartMetroService.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string MerTxnId { get; set; } = string.Empty;
    public string? PgTxnId { get; set; }
    public string? BankTrxId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public string Status { get; set; } = "Pending";
    public string? PaymentProcessor { get; set; }
    public DateTime? PaidAt { get; set; }
    public int? FromStationId { get; set; }
    public int? ToStationId { get; set; }
    public int PaymentFor { get; set; }
    public int TicketType { get; set; } = (int)SmartMetroService.Domain.Entities.TicketType.SingleJourney;
    public bool TicketGenerated { get; set; }
}
