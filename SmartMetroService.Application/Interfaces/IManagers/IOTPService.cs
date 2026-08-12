using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IOTPService
{
    Task<string> GenerateEmailVerificationOtp(string email);
    Task<string> GenerateOtpAsync(string email, OtpType type);
    Task<bool> ValidateOtpAsync(string email, string otp, OtpType emailVerification);
    Task<bool> VerifyOtpAsync(OtpVerificationDto otpVerificationDto);
    Task<bool> SendOtpToEmailAsync(OtpVerificationDto sendOtpData);
}
