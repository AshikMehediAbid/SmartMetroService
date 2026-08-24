using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using System.Security.Claims;

namespace SmartMetroService.Api.Controllers;

[Route("api/wallet")]
[ApiController]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    [Route("balance")]
    [Authorize]
    public async Task<IActionResult> GetUserBalance()
    {
        try
        {
            var email = User.FindFirst("email")?.Value ?? User.FindFirstValue(ClaimTypes.Email);

            var balance = await _walletService.GetBalanceByEmailAsync(email);
            return Ok(balance);
        }
        catch(UnauthorizedException ex)
        {
            return Unauthorized(ex);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
                 
        }
    }
}
