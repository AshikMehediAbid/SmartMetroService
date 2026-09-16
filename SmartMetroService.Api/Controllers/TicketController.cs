using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using System.Security.Claims;

namespace SmartMetroService.Api.Controllers;

[Route("api/tickets")]
[ApiController]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IPdfService _pdfService;

    public TicketController(ITicketService ticketService, IPdfService pdfService)
    {
        _ticketService = ticketService;
        _pdfService = pdfService;
    }


    [Authorize]
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetUserTickets([FromQuery]TicketStatus ticketStatus = TicketStatus.Fresh)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var tickets = await _ticketService.GetTicketsOfAUserByTicketStatus(userEmail, ticketStatus);
        return Ok(tickets);
    }



    [Authorize]
    [HttpGet]
    [Route("rapidpass")]
    public async Task<IActionResult> GetOrCreateUserRapidPass()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        RapidPassResponseDto rapidPass = await _ticketService.GetOrCreateUserRapidPass(userEmail);
        return Ok(rapidPass);
    }



    [HttpGet("download-ticket/{id}")]
    public async Task<IActionResult> DownloadTicket(Guid id)
    {
        try
        {
            var pdfBytes = await _pdfService.GenerateTicketPdfAsync(id);

            var fileName = $"Mrt_Ticket_{id}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest();

        }

    }
}
