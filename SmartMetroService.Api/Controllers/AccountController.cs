using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Api.Models;
using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartMetroService.Api.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";
    private readonly IAccountService _accountService;
    private readonly IProfileService _profileService;
    private readonly IOTPService _oTPService;

    public AccountController(IAccountService accountService, IProfileService profileService, IOTPService oTPService)
    {
        _accountService = accountService;
        _profileService = profileService;
        _oTPService = oTPService;
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
            var (loggedInUser, refreshToken) = await _accountService.LoginUserAsync(user);
            
            if(!string.IsNullOrEmpty(refreshToken))
            {
                SetRefreshTokenCookie(refreshToken);
            }

            return Ok(new ApiResponse<object>()
            {
                Data = new LoginResponse()
                {
                    AccessToken = loggedInUser.AccessToken,
                    IsEmailVerified = loggedInUser.IsEmailVerified,
                    IsEmailSent = loggedInUser.IsEmailSent
                },
                Message = GetLoginSuccessMessage(loggedInUser)
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

    private string GetLoginSuccessMessage(LoginResponse loggedInUser)
    {
        if (loggedInUser.IsEmailVerified == false && loggedInUser.IsEmailSent == true)
            return "Please check your email for verification. An OTP has been sent to your email address.";

        else if (loggedInUser.IsEmailVerified == false && loggedInUser.IsEmailSent == false)
            return "Your email is not verified. Please try again later.";

        else
            return "Login Successful.";
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
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePassword)
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
            await _profileService.ChangePasswordAsync(changePassword, User.FindFirstValue(ClaimTypes.Email));

            return Ok(new ApiResponse<object>()
            {
                Message = "Password changed successfully."
            });

        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>()
            {
                Message = ex.Message
            });
        }
        catch (Exception ex )
        {
            return StatusCode(500, new ApiResponse<object>()
            {
                Message = $"An unexpected error occurred."
            });
        }
        
    }


    [HttpGet]
    [Route("recover-password")]
    public async Task<IActionResult> RecoverPassword(string email)
    {
        try
        {
            var result = await _profileService.RecoverPasswordAsync(email);

            return Ok(new ApiResponse<object>()
            {
                Message = "Check your Email and login with the temporary password."
            });
        }
        catch(NotFoundException ex)
        {
            return NotFound(new ApiResponse<object>()
            {
                Message = ex.Message
            });
        }
        catch(Exception ex)
        {
            return BadRequest(new ApiResponse<object>()
            {
                Message= ex.Message
            });
        }
    }

    [HttpPost]
    [Route("token")]
    public async Task<IActionResult> GetTokens()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new ApiResponse<object>()
            {
                Message = "Refresh token is missing. Please login again."
            });
        }

        try
        {
            TokenDto? tokens = await _accountService.GenerateTokensAsync(refreshToken);
            SetRefreshTokenCookie(tokens?.RefreshToken);

            return Ok(new ApiResponse<object>()
            {
                Data = new 
                {
                    accessToken = tokens?.AccessToken
                },
                Message = "Tokens refreshed successfully"
            });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new ApiResponse<object>()
            {
                Message = ex.Message
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

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        Guid? userId = null;

        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        await _accountService.LogoutAsync(refreshToken, userId);
        RemoveRefreshTokenCookie();

        return Ok(new ApiResponse<object>()
        {
            Message = "Logout successful"
        });
    }

    [HttpGet]
    [Route("user-profile")]
    public async Task<IActionResult> UserProfile(string email)
    {
        try
        {
            var user = await _profileService.GetUserByEmailAsync(email);
            
            if (!user)
            {
                return NotFound(new ApiResponse<object>()
                {
                    Message = "User not found."
                });
            }

            return Ok(new ApiResponse<object>()
            {
                Message = "User profile retrieved successfully."
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

    [HttpPost]
    [Route("Verify-otp")]
    public async Task<IActionResult> VerifyOtp(OtpVerificationDto otpVerificationDto)
    {
        try
        {
            var verifyOtp = await _oTPService.VerifyOtpAsync(otpVerificationDto);

            return Ok(verifyOtp);
        }
        catch
        {
            return BadRequest(new ApiResponse<object>());
        }
    }

    [HttpPost]
    [Route("resend-otp")]
    public async Task<IActionResult> ResendOtp(OtpVerificationDto otpVerificationDto)
    {
        try
        {
            var isSent = await _oTPService.SendOtpToEmailAsync(otpVerificationDto);

            return Ok(isSent);
        }
        catch
        {
            return BadRequest(new ApiResponse<object>());
        }
    }


    private void SetRefreshTokenCookie(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            MaxAge = TimeSpan.FromDays(30)
        });
    }

    private void RemoveRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.None
        });
    }
}
