using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class RapidPassRepository : Repository<RapidPass>, IRapidPassRepository
{
    public RapidPassRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public async Task<RapidPass?> GetByUserIdAsync(Guid id)
    {
        var rapidPass = await _db.RapidPasses.FirstOrDefaultAsync(x => x.UserId == id);

        return rapidPass;
    }
}
