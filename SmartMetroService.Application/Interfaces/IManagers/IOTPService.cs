using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IOTPService
{
    Task<string> GenerateEmailVerificationOtp(string email);
    Task<bool> ValidateOtpAsync(string email, string otp, OtpType emailVerification);
}
