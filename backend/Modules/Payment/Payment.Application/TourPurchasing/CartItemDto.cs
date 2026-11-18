namespace Payment.Application.TourPurchasing;

public sealed record CartItemDto(Guid EntityId, string Name, int Price);
