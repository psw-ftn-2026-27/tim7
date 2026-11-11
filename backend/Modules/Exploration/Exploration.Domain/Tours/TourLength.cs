using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TourLength
{
    public decimal Kilometers { get; private init; }

    private TourLength() { }

    public TourLength(decimal kilometers)
    {
        if (kilometers <= 0)
        {
            throw new DomainException("A tour length requires a positive number of kilometers.");
        }
        if (decimal.Round(kilometers, 1) != kilometers)
        {
            throw new DomainException("A tour length allows at most one decimal place.");
        }

        Kilometers = kilometers;
    }
}
