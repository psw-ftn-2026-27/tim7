using AutoMapper;
using Payment.Application.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletBrowsing;

public sealed class WalletBrowsingQueries
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public WalletBrowsingQueries(IWalletRepository walletRepository, IMapper mapper)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public async Task<WalletDto> GetMyWalletAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            throw new NotFoundException("The user has no wallet.");
        }

        return _mapper.Map<WalletDto>(wallet);
    }
}
