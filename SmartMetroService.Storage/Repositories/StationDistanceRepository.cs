using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class StationDistanceRepository : Repository<StationDistance>, IStationDistanceRepository
{
    public StationDistanceRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public async Task AddStationDistanceAsync(StationDistance entity)
    {
        var stationDistanceEntity = new StationDistance()
        {
            FromStationId = entity.FromStationId,
            ToStationId = entity.ToStationId,
            Distance = entity.Distance
        };

        await _dbSet.AddAsync(stationDistanceEntity);
    }

    public async Task<bool> StationDistanceAlreadyAddedAsync(StationDistance entity)
    {
        bool isExist = await _dbSet
            .AnyAsync(sd =>
                sd.FromStationId == entity.FromStationId &&
                sd.ToStationId == entity.ToStationId &&
                sd.Distance == entity.Distance);

        return isExist;
    }
}
