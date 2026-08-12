namespace SmartMetroService.Application.Models;

public class StationCreationDto : StationInfoDto
{
    public int InsertAfter { get; set; } = 0;
    public double DistanceFromPreviousStation { get; set; } = 0.0;
    public double DistanceFromNextStation { get; set; } = 0.0;


}
