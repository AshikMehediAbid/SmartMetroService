namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository AccountRepository { get; }
    IUserOTPRepository UserTokenRepository { get; }

    Task<int> CompleteAsync();
}
