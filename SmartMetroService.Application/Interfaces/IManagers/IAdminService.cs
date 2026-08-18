using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IAdminService
{
    Task<Settings> GetSystemSettings();
    Task UpdateSystemSettings(Settings settings);
}
