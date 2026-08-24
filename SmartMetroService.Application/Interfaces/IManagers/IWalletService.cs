namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IWalletService
{
    Task DecreaseAccountBalanceAsync(string userEmail, int fare);
    Task<double> GetBalanceByEmailAsync(string email);
}
