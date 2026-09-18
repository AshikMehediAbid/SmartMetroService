using AutoMapper;
using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class StationService : IStationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<bool> CreateStationAsync(StationCreationDto stationCreationDto)
    {
        try
        {
            bool isExist = await _unitOfWork.StationRepository.StationAlreadyExistsByNameAsync(stationCreationDto.StationName);

            if (isExist)
            {
                // await AddNewStationDistance(stationCreationDto);
                // await _unitOfWork.CompleteAsync();

                throw new AlreadyExistsException("Station Already Exist");
            }

            var newStationOrder = stationCreationDto.InsertAfter + 1;

            var newStatonEntity = _mapper.Map<Station>(stationCreationDto);
            newStatonEntity.StationOrder = newStationOrder;

            await UpdateNextStationsOrder(newStationOrder);

            var station = await _unitOfWork.StationRepository.AddAsync(newStatonEntity);
            await _unitOfWork.CompleteAsync();

            await AddNewStationDistance(stationCreationDto);
            await _unitOfWork.CompleteAsync();

            return true;

        }
        catch (AlreadyExistsException)
        {
            throw new AlreadyExistsException("Station Already Exist");
        }
        catch
        {
            return false;
        }
    }

    public async Task<StationResponseDto?> UpdateStationAsync(StationUpdateDto stationUpdateDto)
    {
        var station = await _unitOfWork.StationRepository.GetStationByIdAsync(stationUpdateDto.StationId);
        if (station is null)
            return null;

        await ValidateStationUpdateAsync(stationUpdateDto);

        var stations = await GetStationsInNewOrderAsync(station, stationUpdateDto.InsertAfter);

        // Update the station's editable fields and assign the new order to every station.
        _mapper.Map(stationUpdateDto, station);
        SetStationOrder(stations);

        // Replace the two links connected to this station with the submitted distances.
        await ReplaceStationDistancesAsync(station, stations, stationUpdateDto);

        await _unitOfWork.CompleteAsync();

        return await BuildStationResponseAsync(station);
    }

    private async Task<StationResponseDto> BuildStationResponseAsync(Station station)
    {
        var response = _mapper.Map<StationResponseDto>(station);
        var stationIndex = station.StationOrder - 1;
        var stations = (await _unitOfWork.StationRepository.GetAllAsync())
            .OrderBy(currentStation => currentStation.StationOrder)
            .ToList();

        if (stationIndex > 0)
        {
            response.DistanceFromPreviousStation = await _unitOfWork.StationDistanceRepository
                .GetDistanceByConsicutiveStationAsync(
                    stations[stationIndex - 1].StationId,
                    station.StationId) ?? 0;
        }

        if (stationIndex < stations.Count - 1)
        {
            response.DistanceFromNextStation = await _unitOfWork.StationDistanceRepository
                .GetDistanceByConsicutiveStationAsync(
                    station.StationId,
                    stations[stationIndex + 1].StationId) ?? 0;
        }

        return response;
    }

    private async Task ValidateStationUpdateAsync(StationUpdateDto stationUpdateDto)
    {
        if (await _unitOfWork.StationRepository.StationAlreadyExistsByNameAsync(
                stationUpdateDto.StationName,
                stationUpdateDto.StationId))
        {
            throw new AlreadyExistsException("Station Already Exist");
        }
    }

    private async Task<List<Station>> GetStationsInNewOrderAsync(Station station, int insertAfter)
    {
        var allStations = await _unitOfWork.StationRepository.GetAllAsync();

        var stations = allStations
            .Where(currentStation => currentStation.StationId != station.StationId)
            .OrderBy(currentStation => currentStation.StationOrder)
            .ToList();

        if (insertAfter < 0 || insertAfter > stations.Count)
            throw new ArgumentOutOfRangeException(nameof(insertAfter));

        // Remove the station from its old position, then insert it after the requested number of stations.
        stations.Insert(insertAfter, station);
        return stations;
    }

    private static void SetStationOrder(List<Station> stations)
    {
        for (var index = 0; index < stations.Count; index++)
        {
            stations[index].StationOrder = index + 1;
        }
    }

    private async Task ReplaceStationDistancesAsync(
        Station station,
        List<Station> stations,
        StationUpdateDto stationUpdateDto)
    {
        await _unitOfWork.StationDistanceRepository.DeleteByStationIdAsync(station.StationId);

        var stationIndex = stations.FindIndex(s => s.StationId == station.StationId);
        var previousStation = stationIndex > 0 ? stations[stationIndex - 1] : null;
        var nextStation = stationIndex < stations.Count - 1 ? stations[stationIndex + 1] : null;

        if (previousStation is not null)
        {
            await _unitOfWork.StationDistanceRepository.AddStationDistanceAsync(new StationDistance
            {
                FromStationId = previousStation.StationId,
                ToStationId = station.StationId,
                Distance = stationUpdateDto.DistanceFromPreviousStation
            });
        }

        if (nextStation is not null)
        {
            await _unitOfWork.StationDistanceRepository.AddStationDistanceAsync(new StationDistance
            {
                FromStationId = station.StationId,
                ToStationId = nextStation.StationId,
                Distance = stationUpdateDto.DistanceFromNextStation
            });
        }
    }

    public async Task<bool> DeleteStationAsync(int stationId)
    {
        var station = await _unitOfWork.StationRepository.GetStationByIdAsync(stationId);

        if (station is null)
            return false;

        var stations = await _unitOfWork.StationRepository.GetAllAsync();
        foreach (var remainingStation in stations.Where(s => s.StationOrder > station.StationOrder))
        {
            remainingStation.StationOrder--;
        }

        await _unitOfWork.StationDistanceRepository.DeleteByStationIdAsync(stationId);
        await _unitOfWork.StationRepository.DeleteAsync(station);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    public async Task<List<StationResponseDto>?> GetAllStationAsync(int orderBy)
    {
        var stationEntity = await _unitOfWork.StationRepository.GetAllStationOrderBy(orderBy);
        var stations = new List<StationResponseDto>();

        foreach (var station in stationEntity)
        {
            // Add the distances for the two links surrounding this station.
            stations.Add(await BuildStationResponseAsync(station));
        }

        return stations;

    }

    public async Task<List<StationFareDto>>? GetFare(int fromStationId, int toStationId)
    {
        Station? fromStationData = await _unitOfWork.StationRepository.GetStationByIdAsync(fromStationId);

        if (fromStationData is null)
            return new List<StationFareDto>();

        var toStationData = await _unitOfWork.StationRepository.GetStationByIdAsync(toStationId);

        var stationFare = new List<StationFareDto>();

        if (toStationData is not null)
        {
            var minOrder = Math.Min(fromStationData.StationOrder, toStationData.StationOrder);
            var maxOrder = Math.Max(fromStationData.StationOrder, toStationData.StationOrder);

            double fixedStationDistance = await CalculateFixedToStationDistance(minOrder, maxOrder);
            var settings = await _unitOfWork.AdminRepository.GetSettingsAsync();

            var fixedTwoStationFare = new StationFareDto()
            {
                FromStation = fromStationData.StationName,
                ToStation = toStationData.StationName,
                Distance = Math.Round(fixedStationDistance, 2),
                Fare = CalculateFare(settings.UnitFare, fixedStationDistance, settings.MinimumFare)
            };

            stationFare.Add(fixedTwoStationFare);
            return stationFare;
        }



        stationFare = await CalculateStationFare(fromStationData);

        return stationFare;

    }

    private async Task<double> CalculateFixedToStationDistance(int minOrder, int maxOrder)
    {
        double totalDistance = 0;

        var minOrderedStationData = await _unitOfWork.StationRepository.GetStationByOrderAsync(minOrder);

        for (int i = minOrder + 1; i<= maxOrder; i++)
        {
            var station = await _unitOfWork.StationRepository.GetStationByOrderAsync(i);

            var consicutiveDistance = await CalculateStationDistance(minOrderedStationData.StationId, station.StationId);

            totalDistance += consicutiveDistance;
            minOrderedStationData = station;

        }

        return totalDistance;
    }

    private async Task<List<StationFareDto>> CalculateStationFare(Station fromStationData)
    {
        var stations = await _unitOfWork.StationRepository.GetAllStationOrderBy(1);
        var stationFare = new List<StationFareDto>();
        double cumsum = 0;
        var settings = await _unitOfWork.AdminRepository.GetSettingsAsync();
        var lastStationId = fromStationData.StationId;

        for (int i = fromStationData.StationOrder - 1; i > 0; i--)
        {
            var station = await _unitOfWork.StationRepository.GetStationByOrderAsync(i);

            var distance = await CalculateStationDistance(lastStationId, station.StationId);

            var upperFare = new StationFareDto()
            {
                FromStation = fromStationData.StationName,
                ToStation = station.StationName,
                Distance = Math.Round(distance + cumsum, 2),
                Fare = CalculateFare(settings.UnitFare, distance + cumsum, settings.MinimumFare)
            };

            stationFare.Add(upperFare);
            cumsum += distance;
            lastStationId = station.StationId;
        }

        stationFare.Reverse();

        cumsum = 0;
        lastStationId = fromStationData.StationId;
        for (int i = fromStationData.StationOrder + 1; i <= stations.Count(); i++)
        {
            var station = await _unitOfWork.StationRepository.GetStationByOrderAsync(i);

            var distance = await CalculateStationDistance(lastStationId, station.StationId);

            var upperFare = new StationFareDto()
            {
                FromStation = fromStationData.StationName,
                ToStation = station.StationName,
                Distance = Math.Round(distance + cumsum, 2),
                Fare = CalculateFare(settings.UnitFare, distance + cumsum, settings.MinimumFare)
            };

            stationFare.Add(upperFare);
            cumsum += distance;
            lastStationId = station.StationId;
        }

        return stationFare;
    }

    private int CalculateFare(int unitFare, double distance, int minimumFare)
    {
        var fare = (Math.Max((unitFare * distance), minimumFare));

        var roundupTo10 = (int)(Math.Round(fare / 10) * 10);

        return roundupTo10;
    }

    private async Task<double> CalculateStationDistance(int stationId1, int stationId2)
    {
        var distance = await GetDistanceByConsicutiveStation(stationId1, stationId2) ??
                        await GetDistanceByConsicutiveStation(stationId2, stationId1);

        return distance ?? 0;
    }

    private async Task<double?> GetDistanceByConsicutiveStation(int stationId1, int stationId2)
    {
        return await _unitOfWork.StationDistanceRepository.GetDistanceByConsicutiveStationAsync(stationId1, stationId2);
    }

    private async Task AddNewStationDistance(StationCreationDto stationCreationDto)
    {
        Station? previousStation = await _unitOfWork.StationRepository.GetStationByOrderAsync(stationCreationDto.InsertAfter);
        Station? newStation = await _unitOfWork.StationRepository.GetStationByOrderAsync(stationCreationDto.InsertAfter + 1);
        Station? nextStation = await _unitOfWork.StationRepository.GetStationByOrderAsync(stationCreationDto.InsertAfter + 2);

        if (previousStation is not null)
        {
            var stationDistanceEntity = new StationDistance()
            {
                FromStationId = previousStation.StationId,
                ToStationId = newStation.StationId,
                Distance = stationCreationDto.DistanceFromPreviousStation
            };

            if (!await _unitOfWork.StationDistanceRepository.StationDistanceAlreadyAddedAsync(stationDistanceEntity))
                await _unitOfWork.StationDistanceRepository.AddStationDistanceAsync(stationDistanceEntity);
        }

        if (nextStation is not null)
        {
            var stationDistanceEntity = new StationDistance()
            {
                FromStationId = newStation.StationId,
                ToStationId = nextStation.StationId,
                Distance = stationCreationDto.DistanceFromNextStation
            };

            if (!await _unitOfWork.StationDistanceRepository.StationDistanceAlreadyAddedAsync(stationDistanceEntity))
                await _unitOfWork.StationDistanceRepository.AddStationDistanceAsync(stationDistanceEntity);
        }
    }

    private async Task UpdateNextStationsOrder(int startFrom)
    {
        await _unitOfWork.StationRepository.UpdateStationsOrderAsync(startFrom);
        return;
    }
}
