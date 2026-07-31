using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IAccountService
{
    Task<TokenDto?> GenerateTokensAsync(string refreshToken);
    Task<LoginResponse> LoginUserAsync(LoginUserDto user);
    Task<RegisterUserDto> RegisterNewUserAsync(RegisterUserDto user);
    Task<bool> VerifyEmailAsync(string email, string otp);
}
