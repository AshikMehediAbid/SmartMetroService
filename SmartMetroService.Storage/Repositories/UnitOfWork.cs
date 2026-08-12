using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MyApplicationDbContext _db;

    public IAccountRepository AccountRepository { get; }
    public IUserOTPRepository UserOtpRepository { get; }

    public ITokenRepository TokenRepository { get; }

    public UnitOfWork(MyApplicationDbContext db,
        IAccountRepository accountRepo,
        IUserOTPRepository userOtpRepository,
        ITokenRepository tokenRepo)
    {
        _db = db;

        AccountRepository = accountRepo;
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
