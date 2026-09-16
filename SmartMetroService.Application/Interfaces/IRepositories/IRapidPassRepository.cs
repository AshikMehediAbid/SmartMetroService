using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IRapidPassRepository : IRepository<RapidPass>
{
    Task<RapidPass?> GetByUserIdAsync(Guid id);
}
