using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class OTPService : IOTPService
{
    private readonly IUnitOfWork _unitOfWork;

    public OTPService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GenerateEmailVerificationOtp(string email)
    {
        string token = await OtpGenerator(email, OtpType.EmailVerification, DateTime.UtcNow.AddMinutes(10));

        return token;
    }

    public async Task<bool> ValidateOtpAsync(string email, string otp, OtpType emailVerification)
    {
        var otpData = await _unitOfWork.UserTokenRepository.GetByEmailAsync(email, emailVerification);

        if(otpData == null)
        {
            return false;
        }

        otpData.IsUsed = true;
        return true;
    }

    private async Task<string> OtpGenerator(string email, OtpType otpType, DateTime lifeTime)
    {
        Random random = new Random();
        int number = random.Next(100000, 1000000);
        var otp = number.ToString();

        var userOTP = new UserOTP
        {
            Email = email,
            Otp = otp,
            Type = otpType,
            ExpiryDate = lifeTime,
        };

        await _unitOfWork.UserTokenRepository.AddAsync(userOTP);
        await _unitOfWork.CompleteAsync();

        return otp;
    }
}
