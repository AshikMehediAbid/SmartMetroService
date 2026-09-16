namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository AccountRepository { get; }
    IStationRepository StationRepository { get; }
    IStationDistanceRepository StationDistanceRepository { get; }
    IUserOTPRepository UserOtpRepository { get; }
    ITokenRepository TokenRepository { get; }
    IAdminRepository AdminRepository { get; }
    IWalletRepository WalletRepository { get; }
    ITicketRepository TicketRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    IRapidPassRepository RapidPassRepository { get; }


    Task<int> CompleteAsync();
}
