using System.Text.Json.Serialization;

namespace SmartMetroService.Application.Models;

public class TokenDto
{
    public string AccessToken { get; set; }

    [JsonIgnore]
    public string? RefreshToken { get; set; }
}
