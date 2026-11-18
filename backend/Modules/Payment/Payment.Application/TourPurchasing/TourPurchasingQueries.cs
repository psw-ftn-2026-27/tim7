using AutoMapper;
using Exploration.Contracts;
using Payment.Application.Carts;
using Shared.Domain.Exceptions;

namespace Payment.Application.TourPurchasing;

public sealed class TourPurchasingQueries
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IExplorationApi _explorationApi;
    private readonly IMapper _mapper;

    public TourPurchasingQueries(IShoppingCartRepository cartRepository, IExplorationApi explorationApi, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _explorationApi = explorationApi;
        _mapper = mapper;
    }

    public async Task<TourOfferDto> GetTourOfferAsync(Guid entityId)
    {
        var tour = await _explorationApi.GetTourForSaleAsync(entityId);
        if (tour is null)
        {
            throw new NotFoundException("The tour does not exist.");
        }

        return new TourOfferDto(tour.Name, tour.Amount);
    }

    public async Task<CartDto> GetMyCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            return new CartDto([], 0);
        }

        return new CartDto(_mapper.Map<List<CartItemDto>>(cart.Items), cart.TotalPrice());
    }
}
