using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IUserOTPRepository : IRepository<UserOTP>
{
    Task<UserOTP> GetByEmailAsync(string email, OtpType emailVerification);
}
