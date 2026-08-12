using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using System.Xml.Linq;

namespace SmartMetroService.Application.Managers;

public class OTPService : IOTPService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public OTPService(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<string> GenerateEmailVerificationOtp(string email)
    {
        string otp = await OtpGenerator(email, OtpType.EMAIL_VERIFICATION, DateTime.UtcNow.AddMinutes(10));

        return otp;
    }

    public async Task<string> GenerateOtpAsync(string email, OtpType type)
    {
        DateTime lifeTime = (type == OtpType.PASSWORD_RECOVERY) ? DateTime.UtcNow.AddMinutes(30) : DateTime.UtcNow.AddMinutes(10);

        string otp = await OtpGenerator(email, type, lifeTime);

        return otp;
    }

    public async Task<bool> SendOtpToEmailAsync(OtpVerificationDto sendOtpData)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(sendOtpData.Email);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var otp = await GenerateEmailVerificationOtp(sendOtpData.Email);

        var isSent = await _emailService.
            SendEmailAsync(
                email: sendOtpData.Email,
                subject: "Smart Metro Service - Email Verification OTP",
                message: $"Dear {user.Name}, Your Smart Metro Service account verification OTP is - {otp}. " +
                $"It will be valid for the next 10 minutes. Do NOT share this OTP with anyone."
            );

        return isSent;
    }

    public async Task<bool> ValidateOtpAsync(string email, string otp, OtpType emailVerification)
    {
        var otpData = await _unitOfWork.UserOtpRepository.GetByEmailAsync(email, emailVerification);

        if (otpData == null)
        {
            return false;
        }

        otpData.IsUsed = true;
        return true;
    }

    public async Task<bool> VerifyOtpAsync(OtpVerificationDto otpVerificationDto)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(otpVerificationDto.Email);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        UserOTP? otpData = await _unitOfWork.UserOtpRepository.GetOtpDataAsync(otpVerificationDto);

        if (otpData == null)
        {
            throw new Exception("OTP is expire or Something is wrong. Try again");
        }

        if (otpVerificationDto.Type == OtpType.PASSWORD_RECOVERY)
        {
            
        }
        else if(otpVerificationDto.Type == OtpType.EMAIL_VERIFICATION)
        {
            user.IsEmailVerified = true;
            otpData.IsUsed = true;

            await _unitOfWork.CompleteAsync();
        }

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

        await _unitOfWork.UserOtpRepository.AddAsync(userOTP);
        await _unitOfWork.CompleteAsync();

        return otp;
    }
}
