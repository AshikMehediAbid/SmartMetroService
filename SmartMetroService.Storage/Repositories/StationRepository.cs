using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class StationRepository : Repository<Station>, IStationRepository
{
    public StationRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public async Task<Station?> GetStationByOrderAsync(int order)
    {
        var station = await _dbSet.FirstOrDefaultAsync(s => s.StationOrder == order);
        return station;
    }

    public async Task<bool> StationAlreadyExistsByNameAsync(string stationName)
    {
        var isExist = await _dbSet
            .AnyAsync(s => s.StationName == stationName);

        return isExist;
    }

    public async Task UpdateStationsOrderAsync(int startFrom)
    {
        var x = await _dbSet
            .Where(s => s.StationOrder >= startFrom)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(
                    s => s.StationOrder,
                    s => s.StationOrder + 1
                ));
    }
}
