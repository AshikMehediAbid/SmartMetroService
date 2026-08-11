using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class UserOTPRepository : Repository<UserOTP>, IUserOTPRepository
{
    public UserOTPRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public async Task<UserOTP?> GetByEmailAsync(string email, OtpType otpType)
    {
        var data = await _dbSet.FirstOrDefaultAsync(o => o.Email == email && o.Type == otpType && o.IsUsed == false && o.ExpiryDate >= DateTime.UtcNow);

        return data;
    }

    public async Task<UserOTP?> GetOtpDataAsync(OtpVerificationDto otpModel)
    {
        var data = await _dbSet.FirstOrDefaultAsync(
            o => o.Email == otpModel.Email &&
            o.Otp == otpModel.Otp &&
            o.Type == otpModel.Type &&
            o.IsUsed == false &&
            o.ExpiryDate >= DateTime.UtcNow);

        return data;
    }
}
