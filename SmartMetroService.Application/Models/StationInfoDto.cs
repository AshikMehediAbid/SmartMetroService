using System.ComponentModel.DataAnnotations;

namespace SmartMetroService.Application.Models;

public class StationInfoDto
{
    [Required]
    public string StationName { get; set; }
    public string StationLocation { get; set; }
    public decimal Lat {  get; set; }
    public decimal Long { get; set; }
    public bool IsActive { get; set; } = true;
}
