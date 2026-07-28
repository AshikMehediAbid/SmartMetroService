using System.ComponentModel.DataAnnotations;

namespace SmartMetroService.Application.Models;

public class LoginUserDto
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string PassWord { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
