using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IStationRepository : IRepository<Station>
{
    Task<List<Station>> GetAllStationOrderBy(int orderBy);
    Task<Station?> GetStationByOrderAsync(int order);
    Task<bool> StationAlreadyExistsByNameAsync(string stationName);
    Task UpdateStationsOrderAsync(int startFrom);
}
