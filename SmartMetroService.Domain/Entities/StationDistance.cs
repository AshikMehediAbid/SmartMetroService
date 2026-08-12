namespace SmartMetroService.Domain.Entities;

public class StationDistance : BaseEntity
{
    public int Id { get; set; }
    public int? FromStationId { get; set; }
    public int? ToStationId { get; set; }
    public double Distance { get; set; }

    // Navigation Property
    public Station? FromStation { get; set; }
    public Station? ToStation { get; set; }
}
