using SmartMetroService.Application.Exceptions;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class WalletService : IWalletService
{
    private IUnitOfWork _unitOfWork;

    public WalletService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task DecreaseAccountBalanceAsync(string email, int fare)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(email);

        if (user is null)
            throw new UnauthorizedException("User Not found");

        var wallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(user.Id);

        wallet.Balance -= fare;

        await _unitOfWork.CompleteAsync();
    }

    public async Task<double> GetBalanceByEmailAsync(string email)
    {
        var user = await _unitOfWork.AccountRepository.GetUserByEmailAsync(email);

        if(user is null)
            throw new UnauthorizedException("User Not found");

        var wallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(user.Id);

        if (wallet is null)
        {
            var entity = new UserWallet()
            {
                UserId = user.Id.ToString(),
                Balance = 0
            };

            await _unitOfWork.WalletRepository.AddAsync(entity);
            await _unitOfWork.CompleteAsync();
        }

        var balance = await _unitOfWork.WalletRepository.GetBalanceByUserIdAsync(user.Id);

        return balance;
    }
}
