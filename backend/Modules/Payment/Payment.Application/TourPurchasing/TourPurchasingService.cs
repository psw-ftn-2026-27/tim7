using Exploration.Contracts;
using Payment.Application.Carts;
using Payment.Application.Tokens;
using Payment.Application.Wallets;
using Payment.Domain.Carts;
using Payment.Domain.Checkout;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.TourPurchasing;

public sealed class TourPurchasingService
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPurchaseTokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExplorationApi _explorationApi;
    private readonly CartCheckout _cartCheckout;

    public TourPurchasingService(
        IShoppingCartRepository cartRepository,
        IWalletRepository walletRepository,
        IPurchaseTokenRepository tokenRepository,
        IUnitOfWork unitOfWork,
        IExplorationApi explorationApi,
        CartCheckout cartCheckout)
    {
        _cartRepository = cartRepository;
        _walletRepository = walletRepository;
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
        _explorationApi = explorationApi;
        _cartCheckout = cartCheckout;
    }

    public async Task AddToCartAsync(Guid userId, Guid entityId)
    {
        var tour = await _explorationApi.GetTourForSaleAsync(entityId);
        if (tour is null)
        {
            throw new NotFoundException("The tour does not exist.");
        }

        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            cart = new ShoppingCart(userId);
            _cartRepository.Add(cart);
        }

        var boughtEntityIds = await GetBoughtEntityIdsAsync(userId);

        _cartCheckout.AddToCart(cart, tour.TourId, tour.Name, tour.Amount, tour.AuthorId, IsForSale(tour), boughtEntityIds);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(Guid userId, Guid entityId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            throw new NotFoundException("The user has no cart.");
        }

        cart.RemoveItem(entityId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PayAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            throw new NotFoundException("The user has no cart.");
        }

        var buyerWallets = await GetOrCreateWalletsAsync([userId]);
        var ownerWallets = await GetOrCreateWalletsAsync(cart.Items.Select(item => item.OwnerId).Distinct().ToList());
        var boughtEntityIds = await GetBoughtEntityIdsAsync(userId);

        var forSaleEntityIds = new List<Guid>();
        foreach (var item in cart.Items)
        {
            var tour = await _explorationApi.GetTourForSaleAsync(item.EntityId);
            if (tour is not null && IsForSale(tour))
            {
                forSaleEntityIds.Add(item.EntityId);
            }
        }

        var tokens = _cartCheckout.Pay(cart, buyerWallets[0], ownerWallets, forSaleEntityIds, boughtEntityIds, DateTime.UtcNow);
        _tokenRepository.AddRange(tokens);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<List<Guid>> GetBoughtEntityIdsAsync(Guid userId)
    {
        var tokens = await _tokenRepository.GetByUserIdAsync(userId);
        return tokens.Select(token => token.EntityId).ToList();
    }

    // A wallet that does not exist yet is created, as when an administrator deposits to a user without one.
    private async Task<List<Wallet>> GetOrCreateWalletsAsync(IReadOnlyCollection<Guid> userIds)
    {
        var wallets = await _walletRepository.GetByUserIdsAsync(userIds);
        foreach (var userId in userIds.Where(userId => wallets.All(wallet => wallet.UserId != userId)))
        {
            var wallet = new Wallet(userId);
            _walletRepository.Add(wallet);
            wallets.Add(wallet);
        }

        return wallets;
    }

    // Published and decided to be paid; IsFree is null while the tour is a draft.
    private static bool IsForSale(TourForSaleDto tour)
    {
        return tour.IsPublished && tour.IsFree == false;
    }
}
