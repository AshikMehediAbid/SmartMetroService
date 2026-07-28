using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Api.Models;
using SmartMetroService.Application.Exceptions;
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
    public async Task<ActionResult<ApiResponse<object>>> RegisterUser([FromBody] RegisterUserDto user)
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
            await _accountService.RegisterNewUserAsync(user);

            return Ok(new ApiResponse<object>()
            {
                Message = "Your account has been created. Please login and verify your email."
            });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new ApiResponse<object>()
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>()
            {
                Message = $"An unexpected error occurred. ex.Message"
            });
        }
    }


    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<ApiResponse<object>>> LoginUser([FromBody] LoginUserDto user)
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
            var loggedInUser = await _accountService.LoginUserAsync(user);

            return Ok(new ApiResponse<object>()
            {
                Message = $"{loggedInUser}, Login successful"
            });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new ApiResponse<object>()
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>()
            {
                Message = $"An unexpected error occurred. {ex.Message}"
            });
        }
    }


    [HttpGet]
    [Route("verifyemail")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail([FromQuery] string email, [FromQuery] string otp)
    {
        try
        {
            var verifyEmail = await _accountService.VerifyEmailAsync(email, otp);

            if (verifyEmail)
            {
                return Ok(new ApiResponse<object>()
                {
                    Message = $"{email}, Your email is verified."
                });
            }

            return BadRequest(new ApiResponse<object>()
            {
                Message = "OTP is expired or something went wrong. Please try again."
            });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new ApiResponse<object>()
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>()
            {
                Message = $"An unexpected error occurred,{ex.Message}"
            });
        }
    }

    [HttpPost]
    [Route("change-password")]
    [Authorize]
    public IActionResult ChangePassword([FromBody] ChangePasswordDto changePassword)
    {
        // TODO
        return Ok(User.Claims.Select(c => new
        {
            c.Type,
            c.Value
        }));
    }
}
