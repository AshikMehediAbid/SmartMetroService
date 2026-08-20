using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Application.Models;

namespace SmartMetroService.Api.Controllers;

[Route("api/tickets")]
[ApiController]
public class TicketController : ControllerBase
{
    [HttpPost]
    [Route("purchase")]
    public IActionResult PurchaseTicket([FromBody] PurchaseTicketRequestDto request)
    {
        return Ok("Under Implementation");
    }
}
