using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IAccountRepository : IRepository<User>
{
    Task<(bool, string?)> UserAlreadyExistsAsync(string email, string userName);
}
