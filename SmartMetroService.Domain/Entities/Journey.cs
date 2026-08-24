namespace SmartMetroService.Domain.Entities;

public class Journey
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TicketId { get; set; }

    public int FromStationId { get; set; }
    public int ToStationId { get; set; }

    public int Fare {  get; set; }

    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public JourneyStatus JourneyStatus { get; set; }


    // Navigation properties
    public Ticket? Ticket { get; set; }
    public Station? FromStation { get; set; }
    public Station? ToStation { get; set; }

}

public enum JourneyStatus
{
    Complete = 1,
    InProgress = 2,
    TimeOut = 3,
}
