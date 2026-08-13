using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Api.Models;
using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;

namespace SmartMetroService.Api.Controllers;

[Route("api/station")]
[ApiController]
public class StationController : ControllerBase
{
    private readonly IStationService _stationService;

    public StationController(IStationService stationService)
    {
        _stationService = stationService;
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateNewStation([FromBody] StationCreationDto newStation)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>()
            {
                Message = "One or more fields are invalid."
            });
        }
        try
        {
            var isCreated = await _stationService.CreateStationAsync(newStation);

            if(isCreated)
            {
                return Ok(new ApiResponse<object>()
                {
                    Message = "New Staton is created"
                });
            }

            return BadRequest(
                new ApiResponse<object>()
                {
                    Message = "Something went wrong"
                });
        }
        catch (AlreadyExistsException ex)
        {
            return Ok(new ApiResponse<object>()
            {
                Message = $"{newStation.StationName} Already exist"
            });
        }
        catch(Exception ex)
        {
            return BadRequest(new ApiResponse<object>()
            {
                Message = ex.Message
            });
        }
    }

    [HttpGet]
    [Route("{orderByHints}")]
    public async Task<IActionResult> GetAllStation(int orderByHints) // 1 = ascending; 0 = descending
    {
        try
        {
            var stations = await _stationService.GetAllStationAsync(orderByHints);

            return Ok(new ApiResponse<object>()
            {
                Data = stations,
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Route("fare")]
    public async Task<IActionResult> StationFare(int fromStationId, int toStationId = 0)
    {
        List<StationFareDto> result = await _stationService.GetFare(fromStationId, toStationId);

        return Ok(result);
    }
}
