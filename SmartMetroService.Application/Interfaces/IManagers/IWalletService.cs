namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IWalletService
{
    Task DecreaseWalletBalanceAsync(string userEmail, int fare);
    Task IncreaseWalletBalanceAsync(string userEmail, decimal amount);
    Task<double> GetWalletBalanceByEmailAsync(string email);
}
