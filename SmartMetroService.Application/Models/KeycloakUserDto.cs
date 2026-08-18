namespace SmartMetroService.Application.Models;

public class KeycloakUserDto
{
    public Guid KeycloakUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
