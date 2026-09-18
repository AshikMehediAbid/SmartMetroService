using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IStationService
{
    public Task<bool> CreateStationAsync(StationCreationDto stationCreationDto);
    Task<StationResponseDto?> UpdateStationAsync(StationUpdateDto stationUpdateDto);
    Task<bool> DeleteStationAsync(int stationId);
    Task<List<StationResponseDto>?> GetAllStationAsync(int orderBy);
    Task<List<StationFareDto>> GetFare(int fromStationId, int toStationId = 0);
}
