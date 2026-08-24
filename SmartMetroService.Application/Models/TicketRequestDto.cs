using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Models;

public class TicketRequestDto
{
    public string UserEmail { get; set; }
    public TicketStatus TicketStatus { get; set; }
}

