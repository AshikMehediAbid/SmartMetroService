namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository AccountRepository { get; }
    IStationRepository StationRepository { get; }
    IStationDistanceRepository StationDistanceRepository { get; }
    IUserOTPRepository UserOtpRepository { get; }
    ITokenRepository TokenRepository { get; }


    Task<int> CompleteAsync();
}
