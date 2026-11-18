using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletBrowsing;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize]
public sealed class WalletBrowsingController : ControllerBase
{
    private readonly WalletBrowsingQueries _browsingQueries;

    public WalletBrowsingController(WalletBrowsingQueries browsingQueries)
    {
        _browsingQueries = browsingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<WalletDto>> GetMyWallet()
    {
        return await _browsingQueries.GetMyWalletAsync(User.GetUserId());
    }
}
