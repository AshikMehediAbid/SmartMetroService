namespace SmartMetroService.Application.Models;

public class StationResponseDto : StationInfoDto
{
    public int StationId { get; set; }
    public int StationOrder { get; set; }
    public double DistanceFromPreviousStation { get; set; }
    public double DistanceFromNextStation { get; set; }
}