namespace SmartMetroService.Application.Models;

public class StationDetailsDto : StationInfoDto
{
    public int StationId { get; set; }
    public int StationOrder { get; set; }
}
