namespace SmartMetroService.Application.Models;

public class EmailConfiguration
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
}