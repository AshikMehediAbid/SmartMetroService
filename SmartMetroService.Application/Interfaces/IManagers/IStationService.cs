using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IStationService
{
    public Task<bool> CreateStationAsync(StationCreationDto stationCreationDto);
    Task<List<StationDetailsDto>?> GetAllStationAsync(int orderBy);
    Task<List<StationFareDto>> GetFare(int fromStationId, int toStationId = 0);
}
