namespace SmartMetroService.Domain.Entities;

public class Settings : BaseEntity
{
    public int Id { get; set; }
    public int UnitFare { get; set; }
    public int MinimumFare { get; set; }

}
