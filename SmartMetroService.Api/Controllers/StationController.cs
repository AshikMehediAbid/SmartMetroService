using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpDelete]
    [Route("{stationId:int}")]
    public async Task<IActionResult> DeleteStation(int stationId)
    {
        try
        {
            var isDeleted = await _stationService.DeleteStationAsync(stationId);

            if (!isDeleted)
            {
                return NotFound(new ApiResponse<object>()
                {
                    Message = "Station not found"
                });
            }

            return Ok(new ApiResponse<object>()
            {
                Message = "Station deleted successfully"
            });
        }
        catch (DbUpdateException)
        {
            return Conflict(new ApiResponse<object>()
            {
                Message = "Station cannot be deleted because it is already referenced"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>()
            {
                Message = ex.Message
            });
        }
    }

    [HttpPut]
    [Route("{stationId:int}")]
    public async Task<IActionResult> UpdateStation(int stationId, [FromBody] StationUpdateDto stationUpdate)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>()
            {
                Message = "One or more fields are invalid."
            });
        }

        stationUpdate.StationId = stationId;

        try
        {
            var updatedStation = await _stationService.UpdateStationAsync(stationUpdate);

            if (updatedStation is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    Message = "Station not found"
                });
            }

            return Ok(updatedStation);
        }
        catch (AlreadyExistsException)
        {
            return Conflict(new ApiResponse<object>()
            {
                Message = $"{stationUpdate.StationName} Already exist"
            });
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new ApiResponse<object>()
            {
                Message = "InsertAfter must identify a valid position"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>()
            {
                Message = ex.Message
            });
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
