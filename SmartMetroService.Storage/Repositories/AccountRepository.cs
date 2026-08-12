using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class AccountRepository : Repository<User>, IAccountRepository
{
    public AccountRepository(MyApplicationDbContext db) : base(db)
    {

    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _dbSet.FirstOrDefaultAsync(x => x.Email == email);

        return user;
    }

    public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        var user = await _dbSet.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

        return user;
    }

    public async Task<(bool,string?)> UserAlreadyExistsAsync(string email, string phoneNumber)
    {
        var isEmailExist = await _dbSet.AnyAsync(u => u.Email == email);
        if (isEmailExist) return (true,"Email");

        var isPhoneNumberExist = await _dbSet.AnyAsync(u => u.PhoneNumber == phoneNumber);

        return isPhoneNumberExist ? (true, "PhoneNumber") : (false,null);

    }
}
