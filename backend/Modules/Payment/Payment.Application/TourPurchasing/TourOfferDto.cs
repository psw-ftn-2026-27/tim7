namespace Payment.Application.TourPurchasing;

// Price is null for a tour that is not for sale (free or a draft).
public sealed record TourOfferDto(string Name, int? Price);
