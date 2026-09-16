using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Models;

public class RapidPassResponseDto
{
    public TicketType Type { get; set; } = TicketType.RapidPass;
    public string? QrCode { get; set; }
}
