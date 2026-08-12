namespace SmartMetroService.Application.Models;

public class UserProfileDto
{
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string UserPhone { get; set; }
    public string UserImageUrl { get; set; } = string.Empty;
}
