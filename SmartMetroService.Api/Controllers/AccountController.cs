using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;

namespace SmartMetroService.Api.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto user)
    {
        try
        {
            var userRegister = await _accountService.RegisterNewUserAsync(user);

            return (Ok(new
            {
                UserName = user.FullName,
                Message = "Your Account has been created. Login and verify Your Email"
            }));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
