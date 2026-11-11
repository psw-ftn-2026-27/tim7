using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record RecommendedGroupSize
{
    public int From { get; private init; }
    public int To { get; private init; }

    private RecommendedGroupSize() { }

    public RecommendedGroupSize(int from, int to)
    {
        if (from < 1)
        {
            throw new DomainException("A recommended group size requires at least one person.");
        }
        if (from > to)
        {
            throw new DomainException("A recommended group size cannot start above its upper limit.");
        }

        From = from;
        To = to;
    }
}
