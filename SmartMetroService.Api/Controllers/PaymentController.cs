using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;
using System.Security.Claims;

namespace SmartMetroService.Api.Controllers;

[Route("api/payment")]
[ApiController]
public class PaymentController : ControllerBase
{
    private IPaymentService _paymentService;

    private readonly ILogger<PaymentController> _logger;
    private readonly IConfiguration _configuration;
    private readonly string BASEURL;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger, IConfiguration configuration)
    {
        _paymentService = paymentService;
        _logger = logger;
        _configuration = configuration;
        BASEURL = _configuration.GetValue<string>("BaseFeUrl")!;
    }

    [HttpPost]
    [Route("")]
    [Authorize]
    public async Task<IActionResult> PurchaseTicket([FromBody] PurchaseTicketRequestDto request)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(userEmail)
                || !string.Equals(userEmail, request.UserEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Invalid User" });
            }

            var url = await _paymentService.CompletePaymentAsync(request);

            return Ok(new { message = "Payment successful", url = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment initiation failed for {Email}", request.UserEmail);
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPost("success")]
    [AllowAnonymous]
    public async Task<IActionResult> Success([FromForm] string mer_txnid)
    {
        try
        {
            _logger.LogInformation("Payment Success callback received for Transaction: {TransactionId}", mer_txnid);

            if (string.IsNullOrEmpty(mer_txnid))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid transaction ID"
                });
            }

            AamarPayValidationResponse validationResult = await _paymentService.ValidatePaymentAsync(mer_txnid);

            if (validationResult.PayStatus?.Equals(
                    "Successful",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                var finalized = await _paymentService.FinalizePaymentAsync(mer_txnid, validationResult);

                return Redirect($"{BASEURL}?transactionId={mer_txnid}&status={(finalized ? "success" : "failed")}");
            }

            return Redirect($"{BASEURL}?transactionId={mer_txnid}&status=failed");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing success callback for {TransactionId}",
                mer_txnid);

            return Redirect(
                $"{BASEURL}?status=error");
        }
    }

    [HttpPost("fail")]
    [AllowAnonymous]
    public async Task<IActionResult> FailAsync([FromForm] string mer_txnid)
    {
        _logger.LogWarning("Payment Failed for Transaction: {TransactionId}", mer_txnid);

        await _paymentService.UpdatePaymentStatusAsync(mer_txnid, "Failed");
        return Redirect($"{BASEURL}?status=failed&transactionId={mer_txnid}");
    }

    [HttpPost("cancel")]
    [AllowAnonymous]
    public async Task<IActionResult> CancelAsync([FromForm] string mer_txnid)
    {
        _logger.LogInformation("Payment Cancelled for Transaction: {TransactionId}", mer_txnid);

        await _paymentService.UpdatePaymentStatusAsync(mer_txnid, "Cancelled");
        return Redirect($"{BASEURL}?status=cancelled&transactionId={mer_txnid}");
    }



    [HttpGet("status/{merTxnId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentStatus(string merTxnId)
    {
        PaymentStatusResponseDto? payment = await _paymentService.GetPaymentStatusAsync(merTxnId);

        if (payment == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Payment not found"
            });
        }

        return Ok(payment);
    }
}
