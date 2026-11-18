using Shared.Domain.Exceptions;

namespace Exploration.Domain.LocationSearches;

public sealed record SearchRadius
{
    public double Kilometers { get; private init; }

    private SearchRadius() { }

    public SearchRadius(double kilometers)
    {
        if (kilometers <= 0)
        {
            throw new DomainException("A search radius must be a positive number of kilometers.");
        }

        Kilometers = kilometers;
    }
}
