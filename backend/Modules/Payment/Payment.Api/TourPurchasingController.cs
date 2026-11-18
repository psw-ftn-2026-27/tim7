using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.TourPurchasing;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment")]
[Authorize(Roles = "explorer")]
public sealed class TourPurchasingController : ControllerBase
{
    private readonly TourPurchasingService _purchasingService;
    private readonly TourPurchasingQueries _purchasingQueries;

    public TourPurchasingController(TourPurchasingService purchasingService, TourPurchasingQueries purchasingQueries)
    {
        _purchasingService = purchasingService;
        _purchasingQueries = purchasingQueries;
    }

    [HttpGet("tours/{entityId:guid}/offer")]
    public async Task<ActionResult<TourOfferDto>> GetTourOffer(Guid entityId)
    {
        return await _purchasingQueries.GetTourOfferAsync(entityId);
    }

    [HttpGet("cart")]
    public async Task<ActionResult<CartDto>> GetMyCart()
    {
        return await _purchasingQueries.GetMyCartAsync(User.GetUserId());
    }

    [HttpPost("cart/items/{entityId:guid}")]
    public async Task<ActionResult> AddToCart(Guid entityId)
    {
        await _purchasingService.AddToCartAsync(User.GetUserId(), entityId);
        return NoContent();
    }

    [HttpDelete("cart/items/{entityId:guid}")]
    public async Task<ActionResult> RemoveFromCart(Guid entityId)
    {
        await _purchasingService.RemoveFromCartAsync(User.GetUserId(), entityId);
        return NoContent();
    }

    [HttpPost("cart/payment")]
    public async Task<ActionResult> Pay()
    {
        await _purchasingService.PayAsync(User.GetUserId());
        return NoContent();
    }
}
