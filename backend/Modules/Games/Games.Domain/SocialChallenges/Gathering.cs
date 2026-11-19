using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed record Gathering
{
    public int People { get; private set; }
    public int RangeMeters { get; private set; }

    public Gathering(int people, int rangeMeters)
    {
        if (people is < 2 or > 100)
        {
            throw new DomainException("The number of people must be between 2 and 100.");
        }
        if (rangeMeters is < 2 or > 50)
        {
            throw new DomainException("The range must be between 2 and 50 meters.");
        }
        if (Math.PI * rangeMeters * rangeMeters < people)
        {
            throw new DomainException("The range is too small for that many people.");
        }

        People = people;
        RangeMeters = rangeMeters;
    }
}
