using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class WalletRepository : Repository<UserWallet>, IWalletRepository
{
    public WalletRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public async Task<UserWallet?> GetWalletByUserIdAsync(Guid userId)
    {
        var wallet = await _dbSet.FirstOrDefaultAsync(w => userId == userId);

        return wallet;
    }
    public async Task<double> GetBalanceByUserIdAsync(Guid userId)
    {
        var balance = await _dbSet
            .Where(w => w.UserId == userId.ToString())
            .Select(w => w.Balance)
            .FirstOrDefaultAsync();

        return balance;
    }
}
