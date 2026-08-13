namespace SmartMetroService.Application.Models;

public class StationFareDto
{
    public string FromStation { get; set; }
    public string ToStation { get; set; }
    public double Distance { get; set; }
    public int Fare { get; set; }
}
