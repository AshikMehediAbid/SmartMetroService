using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartMetroService.Application.Managers;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _uOW;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IOTPService _otpService;
    private readonly IEmailService _emailService;

    public AccountService(
        IUnitOfWork uOW,
        IMapper mapper,
        IConfiguration configuration,
        IOTPService otpService,
        IEmailService emailService)
    {
        _uOW = uOW;
        _mapper = mapper;
        _configuration = configuration;
        _otpService = otpService;
        _emailService = emailService;
    }


    public async Task<(LoginResponse, string)> LoginUserAsync(LoginUserDto loginUser)
    {
        var user = await _uOW.AccountRepository.GetUserByPhoneNumberAsync(loginUser.PhoneNumber);

        ValidateloginInfo(user, loginUser);

        var jwtToken = GenerateJwtToken(user.Id, user.Name, user.Email, user.PhoneNumber, user.UserRole);

        var loginResponse = new LoginResponse();

        if (!user.IsEmailVerified)
        {
            loginResponse = new LoginResponse
            {
                AccessToken = user.Email,
                IsEmailSent = await SendEmailVerificationOtp(user.Email, user.Name),
            };

            return (loginResponse, string.Empty);
        }

        loginResponse = new LoginResponse
        {
            AccessToken = jwtToken,
            IsEmailVerified = true
        };

        var refreshToken = await CreateNewRefreshTokenAsync(user.Id);

        return (loginResponse, refreshToken );
    }


    private async Task<bool> SendEmailVerificationOtp(string email, string name)
    {
        var otp = await _otpService.GenerateEmailVerificationOtp(email);

        var isSent = await _emailService.
            SendEmailAsync(
                email: email,
                subject: "Smart Metro Service - Email Verification OTP",
                message: $"Dear {name}, Your Smart Metro Service account verification OTP is - {otp}. " +
                $"It will be valid for the next 10 minutes. Do NOT share this OTP with anyone."
            );

        return isSent;
    }

    private void ValidateloginInfo(User? user, LoginUserDto loginInfo)
    {
        if (user == null)
        {
            throw new ValidationException("Invalid phone number.");
        }

        var result = BCrypt.Net.BCrypt.Verify(loginInfo.PassWord, user.HashedPassword);

        if (!result)
        {
            throw new UnauthorizedException("Invalid password.");
        }
    }

    public async Task<RegisterUserDto> RegisterNewUserAsync(RegisterUserDto user)
    {
        var (isExist, registeredData) = await _uOW.AccountRepository.UserAlreadyExistsAsync(user.Email, user.PhoneNumber);

        if (isExist)
            throw new AlreadyExistsException($"{registeredData} is already registered.");

        try
        {
            var userEntity = _mapper.Map<User>(user);

            userEntity.HashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var registerdUserEntity = await _uOW.AccountRepository.AddAsync(userEntity);
            await _uOW.CompleteAsync();

            return _mapper.Map<RegisterUserDto>(registerdUserEntity);
        }
        catch (Exception)
        {
            throw;
        }
    }



    private string GenerateJwtToken(Guid id, string name, string email, string phoneNumber, UserRole userRole)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(ClaimTypes.Name, name?? "no name"),
                new Claim(ClaimTypes.Email, email?? "no email"),
                new Claim(ClaimTypes.MobilePhone, phoneNumber),
                new Claim(ClaimTypes.Role, userRole.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred during login", ex);
        }
    }

    public async Task<bool> VerifyEmailAsync(string email, string otp)
    {
        User? user = await _uOW.AccountRepository.GetUserByEmailAsync(email);

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.IsEmailVerified)
        {
            return true;
        }

        var validateOtp = await _otpService.ValidateOtpAsync(email, otp, OtpType.EmailVerification);

        if (!validateOtp)
        {
            throw new OtpException("The provided OTP is invalid or expired.");
        }

        user.IsEmailVerified = true;

        await _uOW.CompleteAsync();

        return true;
    }

    public async Task LogoutAsync(string? refreshToken, Guid? userId = null)
    {
        if (userId.HasValue)
        {
            await _uOW.TokenRepository.RevokeAllActiveTokensAsync(userId.Value);
            await _uOW.CompleteAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        await _uOW.TokenRepository.RevokeTokenAsync(ComputeSha256(refreshToken));
        await _uOW.CompleteAsync();
    }

    public async Task<TokenDto?> GenerateTokensAsync(string refreshToken)
    {
        var token = await _uOW.TokenRepository.GetTokenAsync(ComputeSha256(refreshToken));

        if (token is null || token.ExpiredAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Unauthorize Access. Need to login again");
        }

        else if (token.RevokedAt != null)
        {
            await _uOW.TokenRepository.RevokeAllActiveTokensAsync(token.UserId);
            await _uOW.CompleteAsync();
            throw new UnauthorizedException("Unauthorize Access. Need to login again");
        }

        string newRefToken = await CreateNewRefreshTokenAsync(token.UserId);

        //Revoke old refresh token
        token.RevokedAt = DateTime.UtcNow;

        await _uOW.CompleteAsync();

        var user = await _uOW.AccountRepository.GetByIdAsync(token.UserId);

        var tokens = new TokenDto()
        {
            AccessToken = GenerateJwtToken(user.Id, user.Name, user.Email, user.PhoneNumber, user.UserRole),
            RefreshToken = newRefToken,
        };

        return tokens;
    }

    public async Task<string> CreateNewRefreshTokenAsync(Guid userId)
    {
        var newRefToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var newTokenEntity = new Token()
        {
            UserId = userId,
            TokenHash = ComputeSha256(newRefToken),
        };

        await _uOW.TokenRepository.RevokeAllActiveTokensAsync(userId);
        await _uOW.TokenRepository.AddAsync(newTokenEntity);
        await _uOW.CompleteAsync();

        return newRefToken;
    }

    private static string ComputeSha256(string value)
    {
        using var sha = SHA256.Create();

        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(bytes);
    }
}
