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

    public UnitOfWork(MyApplicationDbContext db,
        IAccountRepository accountRepo,
        IUserOTPRepository userOtpRepository,
        IStationRepository stationRepository,
        IStationDistanceRepository stationDistanceRepository,
        ITokenRepository tokenRepo){
        _db = db;

        AccountRepository = accountRepo;
        StationRepository = stationRepository;
        StationDistanceRepository = stationDistanceRepository;
        UserOtpRepository = userOtpRepository;
        TokenRepository = tokenRepo;

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
