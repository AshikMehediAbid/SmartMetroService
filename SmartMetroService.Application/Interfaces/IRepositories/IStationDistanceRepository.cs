using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IStationDistanceRepository : IRepository<StationDistance>
{
    public Task AddStationDistanceAsync(StationDistance entity);
    Task<double?> GetDistanceByConsicutiveStationAsync(int stationId1, int stationId2);
    public Task<bool> StationDistanceAlreadyAddedAsync(StationDistance entity);
}
