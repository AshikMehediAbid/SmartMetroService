using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Models;

public class GenerateTicketRequestDto
{
    public int? FromStationId { get; set; }
    public int? ToStationId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int Fare { get; set; }

    public TicketType TicketType { get; set; }
}
