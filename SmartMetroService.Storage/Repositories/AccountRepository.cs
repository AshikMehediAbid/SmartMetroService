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

    public async Task<(bool,string?)> UserAlreadyExistsAsync(string email, string phoneNumber)
    {
        var isEmailExist = await _dbSet.AnyAsync(u => u.Email == email);
        if (isEmailExist) return (true,"Email");

        var isPhoneNumberExist = await _dbSet.AnyAsync(u => u.PhoneNumber == phoneNumber);

        return isEmailExist ? (true, "PhoneNumber") : (false,null);

    }
}
