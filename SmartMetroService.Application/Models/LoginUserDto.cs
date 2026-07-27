namespace SmartMetroService.Application.Models;

public class LoginUserDto
{
    public string PhoneNumber { get; set; }
    public string PassWord { get; set; }
    public bool RememberMe { get; set; }
}
