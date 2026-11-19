namespace Payment.Application.TourPurchasing;

public sealed record CartDto(List<CartItemDto> Items, int TotalPrice);
