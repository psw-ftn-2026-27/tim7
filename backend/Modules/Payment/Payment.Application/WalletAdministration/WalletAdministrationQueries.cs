using AutoMapper;
using Payment.Application.Wallets;
using Shared.Domain;

namespace Payment.Application.WalletAdministration;

public sealed class WalletAdministrationQueries
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public WalletAdministrationQueries(IWalletRepository walletRepository, IMapper mapper)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public async Task<PageResult<UserBalanceDto>> GetUsersWithBalancesAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var wallets = await _walletRepository.GetPageAsync(page, pageSize);
        var totalCount = await _walletRepository.CountAsync();

        return new PageResult<UserBalanceDto>(_mapper.Map<List<UserBalanceDto>>(wallets), totalCount);
    }
}
