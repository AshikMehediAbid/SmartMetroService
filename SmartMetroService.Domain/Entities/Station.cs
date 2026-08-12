namespace SmartMetroService.Domain.Entities;

public class Station : BaseEntity
{
    public int StationId { get; set; }
    public string StationName { get; set; }
    public string StationLocation { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int StationOrder { get; set; }
    public bool IsActive { get; set; }

    public ICollection<StationDistance> FromDistances { get; set; }
        = new List<StationDistance>();

    public ICollection<StationDistance> ToDistances { get; set; }
        = new List<StationDistance>();

}
