namespace Shared.Domain.Chats;

public sealed record ChatParticipant
{
    public Guid UserId { get; private init; }
    public bool IsPrivileged { get; private init; }

    public ChatParticipant(Guid userId, bool isPrivileged)
    {
        UserId = userId;
        IsPrivileged = isPrivileged;
    }
}
