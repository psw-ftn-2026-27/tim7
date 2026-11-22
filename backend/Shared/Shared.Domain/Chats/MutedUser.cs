namespace Shared.Domain.Chats;

public sealed record MutedUser
{
    public Guid UserId { get; private init; }

    private MutedUser() { }

    public MutedUser(Guid userId)
    {
        UserId = userId;
    }
}
