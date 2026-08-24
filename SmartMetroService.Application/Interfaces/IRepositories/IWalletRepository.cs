using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IWalletRepository : IRepository<UserWallet>
{
    Task<double> GetBalanceByUserIdAsync(Guid userId);
    Task<UserWallet?> GetWalletByUserIdAsync(Guid userId);
}
