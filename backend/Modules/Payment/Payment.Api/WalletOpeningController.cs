using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletOpening;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize]
public sealed class WalletOpeningController : ControllerBase
{
    private readonly WalletOpeningService _openingService;

    public WalletOpeningController(WalletOpeningService openingService)
    {
        _openingService = openingService;
    }

    [HttpPost]
    public async Task<ActionResult> OpenWallet()
    {
        await _openingService.OpenWalletAsync(User.GetUserId());
        return NoContent();
    }
}
