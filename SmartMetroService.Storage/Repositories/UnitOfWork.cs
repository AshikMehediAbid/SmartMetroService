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

    public UnitOfWork(MyApplicationDbContext db,
        IAccountRepository accountRepo,
        IUserOTPRepository userTokenRepository,
        IStationRepository stationRepository,
        IStationDistanceRepository stationDistanceRepository)
    {
        _db = db;

        AccountRepository = accountRepo;
        UserTokenRepository = userTokenRepository;
        StationRepository = stationRepository;
        StationDistanceRepository = stationDistanceRepository;
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
