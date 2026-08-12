using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Models;

public class OtpVerificationDto
{
    public string Email { get; set; }
    public string Otp {  get; set; }
    public OtpType Type { get; set; }
}


