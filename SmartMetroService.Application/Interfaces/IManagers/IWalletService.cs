namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IWalletService
{
    Task DecreaseAccountBalanceAsync(string userEmail, int fare);
    Task IncreaseAccountBalanceAsync(string userEmail, decimal amount);
    Task<double> GetBalanceByEmailAsync(string email);
}
