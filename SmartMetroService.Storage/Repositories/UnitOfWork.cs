using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MyApplicationDbContext _db;

    public IAccountRepository AccountRepository { get; }

    public UnitOfWork(MyApplicationDbContext db,
        IAccountRepository accountRepo)
    {
        _db = db;

        AccountRepository = accountRepo;
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
