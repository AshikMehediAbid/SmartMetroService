namespace SmartMetroService.Application.Models;

public class LoginResponse
{
    public string token { get; set; }
    public bool isVerified { get; set; } = false;
    public bool? isSent { get; set; } = null;
}
