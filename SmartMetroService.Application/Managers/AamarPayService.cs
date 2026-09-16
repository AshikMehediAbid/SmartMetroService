using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using System.Text;
using System.Text.Json;

namespace SmartMetroService.Application.Managers;

public class AamarPayService : IAamarPayService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AamarPayService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;


    private readonly string _storeId;
    private readonly string _signatureKey;
    private readonly string _successUrl;
    private readonly string _failUrl;
    private readonly string _cancelUrl;
    private readonly string _sandboxApiUrl;

    public AamarPayService(
        ILogger<AamarPayService> logger,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;

        _storeId = _configuration["AamarPay:StoreId"]!;
        _signatureKey = _configuration["AamarPay:SignatureKey"]!;
        _successUrl = _configuration["AamarPay:CallbackUrls:Success"]!;
        _failUrl = _configuration["AamarPay:CallbackUrls:Fail"]!;
        _cancelUrl = _configuration["AamarPay:CallbackUrls:Cancel"]!;
        _sandboxApiUrl = _configuration["AamarPay:ApiUrl"]!;

    }
    public async Task<string> InitiatePaymentAsync(PurchaseTicketRequestDto paymentRequest)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(paymentRequest.UserEmail);
        try
        {
            var paymentData = new AamarPayRequest
            {
                store_id = _storeId,
                tran_id = Guid.NewGuid().ToString(),
                success_url = _successUrl,
                fail_url = _failUrl,
                cancel_url = _cancelUrl,
                amount = paymentRequest.Amount.ToString(),
                currency = "BDT",
                signature_key = _signatureKey,
                desc = "Book Purchase Payment",
                cus_name = user?.Name ?? "anonymous",
                cus_email = user?.Email ?? "payer@customer.com",
                cus_add1 = "",
                cus_add2 = "Mohakhali DOHS",
                cus_city = "",
                cus_state = "",
                cus_postcode = "",
                cus_country = "Bangladesh",
                cus_phone = user?.PhoneNumber ?? "01999999999",
                type = "json"
            };

            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonSerializer.Serialize(paymentData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_sandboxApiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("AamarPay API call failed: {StatusCode}", response.StatusCode);
                throw new Exception($"Payment initiation failed: {response.StatusCode}");
            }

            var responseData = JsonSerializer.Deserialize<Dictionary<string, string>>(responseString);

            if (responseData != null && responseData.TryGetValue("payment_url", out var paymentUrl))
            {
                await SavePaymentStatus(paymentRequest, paymentData.tran_id);
                return paymentUrl;
            }
            else if (responseData != null && responseData.TryGetValue("errorMessage", out var errorMessage))
            {
                _logger.LogError("AamarPay error: {ErrorMessage}", errorMessage);
                throw new Exception($"Payment initiation failed: {errorMessage}");
            }
            else
            {
                _logger.LogError("Unexpected response from AamarPay: {Response}", responseString);
                throw new Exception("Payment URL not found in response.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment");
            throw;
        }
    }

    private async Task SavePaymentStatus(PurchaseTicketRequestDto paymentRequest, string tran_id)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(paymentRequest.UserEmail)
            ?? throw new InvalidOperationException("User not found");

        await _unitOfWork.PaymentRepository.AddAsync(new Domain.Entities.Payment
        {
            UserId = user.Id,
            UserEmail = user.Email,
            MerTxnId = tran_id,
            Amount = paymentRequest.Amount,
            Currency = "BDT",
            Status = "Pending",
            PaymentProcessor = "AamarPay",
            FromStationId = paymentRequest.FromStationId,
            ToStationId = paymentRequest.ToStationId,
            PaymentFor = (int)(paymentRequest.PaymentFor ?? PaymentFor.SingleJourney),
            TicketType = (int)((paymentRequest.PaymentFor ?? PaymentFor.SingleJourney) == PaymentFor.RapidPass
                ? TicketType.RapidPass
                : TicketType.SingleJourney)
        });

        await _unitOfWork.CompleteAsync();
    }

    public async Task<AamarPayValidationResponse> ValidatePaymentAsync(string merTxnId)
    {
        try
        {
            if (string.IsNullOrEmpty(merTxnId))
            {
                throw new ArgumentException("Transaction ID is required");
            }

            string url = $"{_configuration["AamarPay:ValidationApiUrl"]}?request_id={Uri.EscapeDataString(merTxnId)}&store_id={Uri.EscapeDataString(_storeId)}&signature_key={Uri.EscapeDataString(_signatureKey)}&type=json";

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Payment validation API call failed: {StatusCode}", response.StatusCode);
                throw new Exception($"Payment validation failed: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AamarPayValidationResponse>(json);

            if (result == null)
            {
                throw new Exception("Failed to deserialize validation response");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment");
            throw;
        }
    }
}
