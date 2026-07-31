namespace SmartMetroService.Application.Models;

public class LoginResponse
{
    public TokenDto? tokens { get; set; }
    public bool isVerified { get; set; } = false;
    public bool? isSent { get; set; } = null;
}
