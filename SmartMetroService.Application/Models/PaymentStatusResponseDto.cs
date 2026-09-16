namespace SmartMetroService.Application.Models;

public class PaymentStatusResponseDto
{
    public string TransactionId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string? PaymentMethod { get; set; }

    public string? PgTransactionId { get; set; }

    public string? BankTransactionId { get; set; }

    public DateTime? PaidAt { get; set; }
}
