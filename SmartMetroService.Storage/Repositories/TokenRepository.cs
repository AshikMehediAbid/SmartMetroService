using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;
using System.Security.Cryptography;
using System.Text;

namespace SmartMetroService.Storage.Repositories;

public class TokenRepository : Repository<Token>, ITokenRepository
{
    public TokenRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public async Task<Token?> GetTokenAsync(string hash)
    {
        var token = await _dbSet.FirstOrDefaultAsync(t => t.TokenHash == hash);

        return token;
    }

    public async Task RevokeAllActiveTokensAsync(Guid userId)
    {
        await _dbSet
            .Where(t => t.UserId == userId &&
                        t.RevokedAt == null &&
                        t.ExpiredAt >= DateTime.UtcNow)
            .ExecuteUpdateAsync(update => update
                .SetProperty(t => t.RevokedAt, DateTime.UtcNow));
    }
}
