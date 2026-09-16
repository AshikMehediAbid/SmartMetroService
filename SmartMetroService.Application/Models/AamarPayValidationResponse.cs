using System.Text.Json.Serialization;

namespace SmartMetroService.Application.Models;

public class AamarPayValidationResponse
{
    [JsonPropertyName("pg_txnid")]
    public string? PgTxnId { get; set; }

    [JsonPropertyName("mer_txnid")]
    public string? MerTxnId { get; set; }

    [JsonPropertyName("pay_status")]
    public string? PayStatus { get; set; }

    [JsonPropertyName("status_title")]
    public string? StatusTitle { get; set; }

    [JsonPropertyName("amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("payment_processor")]
    public string? PaymentProcessor { get; set; }

    [JsonPropertyName("bank_trxid")]
    public string? BankTrxId { get; set; }

    [JsonPropertyName("approval_code")]
    public string? ApprovalCode { get; set; }

    [JsonPropertyName("cus_name")]
    public string? CusName { get; set; }

    [JsonPropertyName("cus_email")]
    public string? CusEmail { get; set; }

    [JsonPropertyName("cus_phone")]
    public string? CusPhone { get; set; }

    [JsonPropertyName("desc")]
    public string? Description { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("store_amount")]
    public string? StoreAmount { get; set; }

    [JsonPropertyName("opt_a")]
    public string? OptA { get; set; }
}
