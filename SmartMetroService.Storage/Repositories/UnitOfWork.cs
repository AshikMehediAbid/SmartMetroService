using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MyApplicationDbContext _db;

    public IAccountRepository AccountRepository { get; }
    public IUserOTPRepository UserTokenRepository { get; }
    public IStationRepository StationRepository { get; }
    public IStationDistanceRepository StationDistanceRepository { get; }
    public IUserOTPRepository UserOtpRepository { get; }
    public ITokenRepository TokenRepository { get; }
    public IAdminRepository AdminRepository { get; }
    public IWalletRepository WalletRepository { get; }
    public ITicketRepository TicketRepository { get; }
    public IPaymentRepository PaymentRepository { get; }
    public IRapidPassRepository RapidPassRepository { get; }
    public IJourneyRepository JourneyRepository { get; }

    public UnitOfWork(MyApplicationDbContext db,
        IAccountRepository accountRepo,
        IUserOTPRepository userOtpRepository,
        IStationRepository stationRepository,
        IStationDistanceRepository stationDistanceRepository,
        ITokenRepository tokenRepo,
        IAdminRepository adminRepo,
        IWalletRepository walletRepo,
        ITicketRepository ticketRepo,
        IPaymentRepository paymentRepo,
        IRapidPassRepository rapidPassRepo,
        IJourneyRepository journeyRepo){
        _db = db;

        AccountRepository = accountRepo;
        StationRepository = stationRepository;
        StationDistanceRepository = stationDistanceRepository;
        UserOtpRepository = userOtpRepository;
        TokenRepository = tokenRepo;
        AdminRepository = adminRepo;
        WalletRepository = walletRepo;
        TicketRepository = ticketRepo;
        PaymentRepository = paymentRepo;
        RapidPassRepository = rapidPassRepo;
        JourneyRepository = journeyRepo;

    }

    public async Task<int> CompleteAsync()
    {
        var result = await _db.SaveChangesAsync();

        return result;
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
