namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository AccountRepository { get; }
    IUserOTPRepository UserOtpRepository { get; }
    ITokenRepository TokenRepository { get; }

    Task<int> CompleteAsync();
}
