using Identity.Contracts;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletAdministration;

public sealed class WalletAdministrationService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public WalletAdministrationService(IWalletRepository walletRepository, IUnitOfWork unitOfWork, IIdentityApi identityApi)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task DepositAsync(Guid userId, int amount, Guid administratorId)
    {
        if (!await _identityApi.UserExistsAsync(userId))
        {
            throw new NotFoundException("The user does not exist.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            wallet = new Wallet(userId);
            _walletRepository.Add(wallet);
        }

        wallet.Deposit(administratorId, amount);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReverseDepositAsync(Guid userId, Guid transactionId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            throw new NotFoundException("The user has no wallet.");
        }

        wallet.ReverseDeposit(transactionId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync();
    }
}
