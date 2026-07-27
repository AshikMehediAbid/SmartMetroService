namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string email, string subject, string message);
}
