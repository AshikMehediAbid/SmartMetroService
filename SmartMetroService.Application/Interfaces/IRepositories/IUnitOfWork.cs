namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository AccountRepository { get; }
    IUserOTPRepository UserTokenRepository { get; }
    IStationRepository StationRepository { get; }
    IStationDistanceRepository StationDistanceRepository { get; }

    Task<int> CompleteAsync();
}
