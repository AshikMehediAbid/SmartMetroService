using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Api.Controllers;

[Route("api/admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    [Route("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _adminService.GetSystemSettings();

        return Ok(settings);
    }

    [HttpPut]
    [Route("settings")]
    public async Task<IActionResult> UpdateSettings(Settings settings)
    {
        await _adminService.UpdateSystemSettings(settings);

        return Ok(settings);
    }
}
