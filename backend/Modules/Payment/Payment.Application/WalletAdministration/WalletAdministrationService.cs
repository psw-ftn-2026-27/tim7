using Payment.Application.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletAdministration;

public sealed class WalletAdministrationService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletAdministrationService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task DepositAsync(Guid userId, int amount, Guid administratorId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            throw new NotFoundException("The user has no wallet.");
        }

        wallet.Deposit(administratorId, amount);

        await _unitOfWork.SaveChangesAsync();
    }
}
