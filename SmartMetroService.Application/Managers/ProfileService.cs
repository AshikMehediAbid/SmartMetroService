using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Managers;

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
}
