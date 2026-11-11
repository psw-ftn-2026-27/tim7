using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed record ActiveWindow
{
    public TimeOnly Start { get; private set; }
    public TimeOnly End { get; private set; }

    public ActiveWindow(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
        {
            throw new DomainException("The start of the active window must be before its end.");
        }

        Start = start;
        End = end;
    }
}
