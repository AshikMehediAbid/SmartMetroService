using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IProfileService
{
    Task ChangePasswordAsync(ChangePasswordDto changePasswordDto, string email);

}
