using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IAdminRepository : IRepository<Settings>
{
    Task<Settings> GetSettingsAsync();
}
