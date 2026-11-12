using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletAdministration;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallets")]
[Authorize(Roles = "administrator")]
public sealed class WalletAdministrationController : ControllerBase
{
    private readonly WalletAdministrationService _administrationService;
    private readonly WalletAdministrationQueries _administrationQueries;

    public WalletAdministrationController(
        WalletAdministrationService administrationService,
        WalletAdministrationQueries administrationQueries)
    {
        _administrationService = administrationService;
        _administrationQueries = administrationQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<UserBalanceDto>>> GetUsersWithBalances([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _administrationQueries.GetUsersWithBalancesAsync(page, pageSize);
    }

    [HttpPost("{userId:guid}/deposits")]
    public async Task<ActionResult> Deposit(Guid userId, [FromBody] DepositDto deposit)
    {
        await _administrationService.DepositAsync(userId, deposit.Amount, User.GetUserId());
        return NoContent();
    }
}
