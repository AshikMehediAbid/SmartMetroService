using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IStationDistanceRepository : IRepository<StationDistance>
{
    public Task AddStationDistanceAsync(StationDistance entity);
    public Task<bool> StationDistanceAlreadyAddedAsync(StationDistance entity);
}
