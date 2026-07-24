namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository AccountRepository { get; }

    Task<int> CompleteAsync();
}
