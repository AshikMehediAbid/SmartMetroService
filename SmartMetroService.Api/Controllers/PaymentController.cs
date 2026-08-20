using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;
using System.Security.Claims;

namespace SmartMetroService.Api.Controllers;

[Route("api/payment")]
[ApiController]
public class PaymentController : ControllerBase
{
    private IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> PurchaseTicket([FromBody] PurchaseTicketRequestDto request)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if(userEmail != request.UserEmail)
            {
                return Unauthorized(new { message = "Invalid User" });
            }

            await _paymentService.CompletePaymentAsync(request);

            return Ok(new { message = "Payment successful" });
        }
        catch (Exception ex) 
        {
            return BadRequest();
        }
    }
}
