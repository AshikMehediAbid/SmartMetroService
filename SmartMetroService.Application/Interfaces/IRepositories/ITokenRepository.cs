using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface ITokenRepository : IRepository<Token>
{
    Task<Token?> GetTokenAsync(string hashedRefreshToken);
    Task RevokeTokenAsync(string hashedRefreshToken);
    Task RevokeAllActiveTokensAsync(Guid userId);
}
