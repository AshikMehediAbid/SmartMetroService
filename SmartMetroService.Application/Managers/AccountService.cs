using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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


    public async Task<LoginResponse> LoginUserAsync(LoginUserDto loginUser)
    {
        var user = await _uOW.AccountRepository.GetUserByPhoneNumberAsync(loginUser.PhoneNumber);

        ValidateloginInfo(user, loginUser);

        var jwtToken = GenerateJwtToken(user.Id, user.Name, user.PhoneNumber, user.UserRole);

        if (!user.IsEmailVerified)
        {
            var isSent = await SendEmailVerificetionOtp(user.Email, user.Name);

            return new LoginResponse
            {
                token = jwtToken,
                isSent = isSent
            };
        }

        return new LoginResponse
        {
            token = jwtToken,
            isVerified = true
        };
    }


    private async Task<bool> SendEmailVerificetionOtp(string email, string name)
    {
        var otp = await _otpService.GenerateEmailVerificationOtp(email);

        var isSent = await _emailService.
            SendEmailAsync(
                email: email,
                subject: "Your email verification OTP",
                message: $"Your Smart Metro Service account verification OTP is {otp}. " +
                $"It will be valid for the next 10 minutes. Do NOT share this OTP with anyone."
            );

        return isSent;
    }

    private void ValidateloginInfo(User? user, LoginUserDto loginInfo)
    {
        if (user == null)
        {
            throw new Exception("Invalid Phone number");
        }


        var result = BCrypt.Net.BCrypt.Verify(loginInfo.PassWord, user.HashedPassword);

        if (!result)
        {
            throw new Exception("Invalid Password");
        }
    }

    public async Task<RegisterUserDto> RegisterNewUserAsync(RegisterUserDto user)
    {
        var (isExist, registeredData) = await _uOW.AccountRepository.UserAlreadyExistsAsync(user.Email, user.PhoneNumber);

        if (isExist)
            throw new Exception($"{registeredData} is already registered");

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



    private string GenerateJwtToken(Guid id, string name, string phoneNumber, UserRole userRole)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(ClaimTypes.Name, name?? "no name"),
                new Claim(ClaimTypes.MobilePhone, phoneNumber),
                new Claim(ClaimTypes.Role, userRole.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["jwt:Issuer"],
                audience: _configuration["jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
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

        if(user is null)
        {
            throw new Exception("User not found");
        }

        if (user.IsEmailVerified)
        {
            return true;
        }

        var validateOtp = await _otpService.ValidateOtpAsync(email, otp, OtpType.EmailVerification);

        if (!validateOtp)
            return false;

        user.IsEmailVerified = true;

        await _uOW.CompleteAsync();

        return true;
    }
}
