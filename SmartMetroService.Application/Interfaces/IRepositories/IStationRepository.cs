using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IStationRepository : IRepository<Station>
{
    Task<Station?> GetStationByOrderAsync(int order);
    Task<bool> StationAlreadyExistsByNameAsync(string stationName);
    Task UpdateStationsOrderAsync(int startFrom);
}
