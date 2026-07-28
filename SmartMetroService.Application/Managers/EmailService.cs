using Microsoft.Extensions.Configuration;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;
using System.Net;
using System.Net.Mail;

namespace SmartMetroService.Application.Managers;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    public async Task<bool> SendEmailAsync(string receiverEmail, string subject, string message)
    {
        var mail = GetEmailConfiguration();

        var mailMessage = new MailMessage
        {
            From = new MailAddress(mail.SenderEmail, mail.SenderName),
            Subject = subject,
            Body = message
        };

        mailMessage.To.Add(receiverEmail);

        using var smtpClient = new SmtpClient(mail.Server, mail.Port)
        {
            Credentials = new NetworkCredential(mail.UserName, mail.Password),
            EnableSsl = true
        };

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            return true;
        }
        catch
        {
            return false;
        }
 
    }

    private EmailConfiguration GetEmailConfiguration()
    {
        var emailConfiguration = new EmailConfiguration
        {
            Server = _config["Email:Server"] ?? "",
            Port = int.Parse(_config["Email:Port"] ?? ""),
            UserName = _config["Email:SmtpUserName"] ?? "",
            Password = _config["Email:Password"] ?? "",
            SenderName = _config["Email:SenderName"] ?? "",
            SenderEmail = _config["Email:SenderEmail"] ?? ""
        };

        return emailConfiguration;
    }
}
