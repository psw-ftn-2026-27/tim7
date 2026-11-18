using Identity.Contracts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Payment.Application.Wallets;
using Shared.Domain;

namespace Payment.Application.WalletAdministration;

public sealed class WalletAdministrationQueries
{
    private const int IdentityPageSize = 100;

    private readonly IWalletRepository _walletRepository;
    private readonly IWalletReadRepository _walletReadRepository;
    private readonly IIdentityApi _identityApi;

    public WalletAdministrationQueries(IWalletRepository walletRepository, IWalletReadRepository walletReadRepository, IIdentityApi identityApi)
    {
        _walletRepository = walletRepository;
        _walletReadRepository = walletReadRepository;
        _identityApi = identityApi;
    }

    public Task<List<UserDepositDto>> GetUserDepositsAsync(Guid userId)
    {
        return _walletReadRepository.GetDepositsByUserAsync(userId);
    }

    public async Task<PageResult<UserBalanceDto>> GetUsersWithBalancesAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _identityApi.GetUsersAsync(page, pageSize);
        var wallets = await _walletRepository.GetByUserIdsAsync(users.Items.Select(user => user.Id).ToList());

        var items = users.Items.Select(user => ToBalanceDto(user, wallets.FirstOrDefault(wallet => wallet.UserId == user.Id))).ToList();
        return new PageResult<UserBalanceDto>(items, users.TotalCount);
    }

    // Identity's contract has no search by part of an e-mail, so the matching users are collected
    // by reading its pages one after another. Replace this when the contract offers a search.
    public async Task<PageResult<UserBalanceDto>> SearchUsersWithBalancesAsync(string query, int page, int pageSize)
    {
        query = query.Trim();
        if (query == "")
        {
            return await GetUsersWithBalancesAsync(page, pageSize);
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var matches = new List<UserSummary>();
        var identityPage = 1;
        UserSummaryPage users;
        do
        {
            users = await _identityApi.GetUsersAsync(identityPage, IdentityPageSize);
            matches.AddRange(users.Items.Where(user => user.Email.Contains(query, StringComparison.OrdinalIgnoreCase)));
            identityPage++;
        }
        while (users.Items.Count > 0 && (identityPage - 1) * IdentityPageSize < users.TotalCount);

        var pageOfMatches = matches.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var wallets = await _walletRepository.GetByUserIdsAsync(pageOfMatches.Select(user => user.Id).ToList());

        var items = pageOfMatches.Select(user => ToBalanceDto(user, wallets.FirstOrDefault(wallet => wallet.UserId == user.Id))).ToList();
        return new PageResult<UserBalanceDto>(items, matches.Count);
    }

    private static UserBalanceDto ToBalanceDto(UserSummary user, Wallet? wallet)
    {
        return new UserBalanceDto(user.Id, user.Email, wallet?.Balance() ?? 0);
    }
}
