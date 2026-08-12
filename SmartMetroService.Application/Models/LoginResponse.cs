namespace SmartMetroService.Application.Models;

public class LoginResponse
{
    public string? AccessToken { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public bool? IsEmailSent { get; set; } = null;
}
