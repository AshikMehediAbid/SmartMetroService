namespace SmartMetroService.Domain.Entities;

public class Ticket : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int? FromStationId { get; set; }
    public int? ToStationId { get; set; }

    public int Fare {  get; set; }
    public byte[]? QRByte { get; set; }

    public TicketType TicketType { get; set; }
    public TicketStatus TicketStatus { get; set; }

    public DateTime? ExpiryTime { get; set; }


    // Navigation properties
    public Station? FromStation { get; set; }
    public Station? ToStation { get; set; }
    public ICollection<Journey> Journeys { get; set; } = [];
}


public enum TicketStatus
{
    Fresh = 1,
    Used = 2,
    Expired = 3,
    InUse = 4,
}

public enum TicketType
{
    SingleJourney = 1,
    RapidPass = 2
}