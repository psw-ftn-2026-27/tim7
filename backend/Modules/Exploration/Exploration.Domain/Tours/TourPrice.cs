using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TourPrice
{
    public bool IsFree { get; private init; }
    public decimal? Amount { get; private init; }

    public TourPrice(bool? isFree, decimal? amount)
    {
        if (isFree is null)
        {
            throw new DomainException("A tour price requires a decision whether the tour is free or paid.");
        }
        if (isFree == false && (amount is null || amount <= 0))
        {
            throw new DomainException("A paid tour requires an amount greater than zero.");
        }
        if (isFree == true && amount is not null)
        {
            throw new DomainException("A free tour cannot have an amount.");
        }

        IsFree = isFree.Value;
        Amount = amount;
    }
}
