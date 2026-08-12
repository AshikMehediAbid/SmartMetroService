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
                await AddNewStationDistance(stationCreationDto);
                await _unitOfWork.CompleteAsync();

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

    private async Task AddNewStationDistance(StationCreationDto stationCreationDto)
    {
        Station? previousStation = await _unitOfWork.StationRepository.GetStationByOrderAsync(stationCreationDto.InsertAfter);
        Station? newStation = await _unitOfWork.StationRepository.GetStationByOrderAsync(stationCreationDto.InsertAfter+1);
        Station? nextStation = await _unitOfWork.StationRepository.GetStationByOrderAsync(stationCreationDto.InsertAfter + 2);

        if(previousStation is not null)
        {
            var stationDistanceEntity = new StationDistance()
            {
                FromStationId = previousStation.StationId,
                ToStationId = newStation.StationId,
                Distance = stationCreationDto.DistanceFromPreviousStation
            };

            if (! await _unitOfWork.StationDistanceRepository.StationDistanceAlreadyAddedAsync(stationDistanceEntity))
                await _unitOfWork.StationDistanceRepository.AddStationDistanceAsync(stationDistanceEntity);
        }

        if(nextStation is not null)
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
