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
                UserName = user.Name,
                Message = "Your Account has been created. Login and verify Your Email"
            }));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginUser(LoginUserDto user)
    {
        try
        {
            var loggedInUser = await _accountService.LoginUserAsync(user);

            return Ok(loggedInUser);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpGet]
    [Route("verifyemail")]
    public async Task<IActionResult> VerifyEmail([FromQuery]string email,[FromQuery] string otp)
    {
        try
        {
            var verifyEmail = await _accountService.VerifyEmailAsync(email , otp);

            if (verifyEmail)
                return Ok("Your Email is verified.");

            else
                return BadRequest("OTP is expired Or Something went wrong. Try again");
        }
        catch
        {
            return BadRequest("Something went wrong");
        }
    }
}
