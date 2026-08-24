using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Models;

public class TicketResponseDto
{
    public Guid Id { get; set; }
    public string? FromStationName { get; set; }
    public string? ToStationName { get; set; }
    public int Fare {  get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }

    public TicketStatus TicketStatus { get; set; }
    public TicketType TicketType { get; set; } = TicketType.SingleJourney;


    public string? QrCode { get; set; }

}
