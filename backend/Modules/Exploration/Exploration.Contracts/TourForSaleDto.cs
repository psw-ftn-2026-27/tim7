namespace Exploration.Contracts;

// IsFree is null for a draft; Amount is null for a free tour and for a draft. IsPublished is true only in the Published status.
public sealed record TourForSaleDto(Guid TourId, string Name, Guid AuthorId, bool? IsFree, int? Amount, bool IsPublished);
