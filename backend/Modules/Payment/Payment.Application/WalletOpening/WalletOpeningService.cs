using Payment.Application.Wallets;
using Payment.Domain.Wallets;

namespace Payment.Application.WalletOpening;

public sealed class WalletOpeningService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletOpeningService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task OpenWalletAsync(Guid userId)
    {
        var existing = await _walletRepository.GetByUserIdAsync(userId);
        if (existing is null)
        {
            _walletRepository.Add(new Wallet(userId));
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
