using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IAccountService
{
    Task<RegisterUserDto> RegisterNewUserAsync(RegisterUserDto user);
}
