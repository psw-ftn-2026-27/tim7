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

    [HttpGet("search")]
    public async Task<ActionResult<PageResult<UserBalanceDto>>> SearchUsersWithBalances([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _administrationQueries.SearchUsersWithBalancesAsync(query, page, pageSize);
    }

    [HttpPost("{userId:guid}/deposits")]
    public async Task<ActionResult> Deposit(Guid userId, [FromBody] DepositDto deposit)
    {
        await _administrationService.DepositAsync(userId, deposit.Amount, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{userId:guid}/deposits")]
    public async Task<ActionResult<List<UserDepositDto>>> GetUserDeposits(Guid userId)
    {
        return await _administrationQueries.GetUserDepositsAsync(userId);
    }

    [HttpPost("{userId:guid}/deposits/{transactionId:guid}/reversal")]
    public async Task<ActionResult> ReverseDeposit(Guid userId, Guid transactionId)
    {
        await _administrationService.ReverseDepositAsync(userId, transactionId);
        return NoContent();
    }
}
