using AutoMapper;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _uOW;
    private readonly IMapper _mapper;

    public AccountService(IUnitOfWork uOW, IMapper mapper)
    {
        _uOW = uOW;
        _mapper = mapper;
    }

    public async Task<RegisterUserDto> RegisterNewUserAsync(RegisterUserDto user)
    {
        var (isExist, registeredData) = await _uOW.AccountRepository.UserAlreadyExistsAsync(user.Email, user.PhoneNumber);

        if (isExist)
            throw new Exception($"{registeredData} is already registered");

        try
        {
            var userEntity = _mapper.Map<User>(user);

            var registerdUserEntity = await _uOW.AccountRepository.AddAsync(userEntity);
            await _uOW.CompleteAsync();

            return _mapper.Map<RegisterUserDto>(registerdUserEntity);
        }
        catch (Exception)
        {
            throw;
        }
    }


}
