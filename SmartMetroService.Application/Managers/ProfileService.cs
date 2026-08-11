using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace SmartMetroService.Application.Managers;

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOTPService _otpService;
    private readonly IEmailService _emailService;

    public ProfileService(IUnitOfWork unitOfWork, IOTPService oTPService, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _otpService = oTPService;
        _emailService = emailService;
    }

    public async Task ChangePasswordAsync(ChangePasswordDto changePasswordDto, string email)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(email);

        if(user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.HashedPassword))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
        {
            throw new InvalidOperationException("New password and confirm password do not match.");
        }

        user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

        await _unitOfWork.CompleteAsync();
    }

    public async Task<bool> GetUserByEmailAsync(string email)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(email);
        return user != null;
    }

    public async Task<bool> RecoverPasswordAsync(string email)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(email);

        if(user is null)
        {
            throw new NotFoundException("User not found with this Email");
        }
        if(user.IsEmailVerified is false)
        {
            throw new Exception("Email not verified");
        }

        var otp = await _otpService.GenerateOtpAsync(email, OtpType.PASSWORD_RECOVERY);

        var isSent = await _emailService.
            SendEmailAsync(
                email: email,
                subject: "Smart Metro Service - Temporary Password",
                message: $"Dear {user.Name}, Your Smart Metro Service Password recovery temporary password is - {otp}. " +
                $"It will be valid for the next 30 minutes. Do NOT share this password with anyone."
            );

        if (isSent)
        {
            user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(otp);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        throw new Exception();
    }
}
