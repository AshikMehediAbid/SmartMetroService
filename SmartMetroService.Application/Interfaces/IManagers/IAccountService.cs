using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IAccountService
{
    Task<string> CreateNewRefreshTokenAsync(Guid userId);
    Task<TokenDto?> GenerateTokensAsync(string refreshToken);
    Task<(LoginResponse, string)> LoginUserAsync(LoginUserDto user);
    Task LogoutAsync(string? refreshToken, Guid? userId = null);
    Task<RegisterUserDto> RegisterNewUserAsync(RegisterUserDto user);
    Task<bool> VerifyEmailAsync(string email, string otp);
}
