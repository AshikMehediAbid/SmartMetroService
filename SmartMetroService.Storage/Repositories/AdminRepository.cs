using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class AdminRepository : Repository<Settings>, IAdminRepository
{
    public AdminRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public async Task<Settings> GetSettingsAsync()
    {
        var settings = await _dbSet.FirstOrDefaultAsync();

        return settings;
    }
}
