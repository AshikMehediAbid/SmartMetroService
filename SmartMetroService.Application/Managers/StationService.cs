using AutoMapper;
using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using System.Data;

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

    public async Task<List<StationDetailsDto>?> GetAllStationAsync(int orderBy)
    {
        var stationEntity = await _unitOfWork.StationRepository.GetAllStationOrderBy(orderBy);

        var stations = _mapper.Map<List<StationDetailsDto>>(stationEntity);

        return stations;

    }

    public async Task<List<StationFareDto>>? GetFare(int fromStationId, int toStationId)
    {
        Station? fromStationData = await _unitOfWork.StationRepository.GetStationByIdAsync(fromStationId);

        if (fromStationData is null)
            return new List<StationFareDto>();

        var toStationData = await _unitOfWork.StationRepository.GetStationByIdAsync(toStationId);

        var stationFare = new List<StationFareDto>();
        
        stationFare = await CalculateStationFare(fromStationData);

        return stationFare;
        
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
