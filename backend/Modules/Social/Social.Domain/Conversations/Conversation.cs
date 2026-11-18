using Shared.Domain;
using Shared.Domain.Chats;
using Shared.Domain.Exceptions;

namespace Social.Domain.Conversations;

public sealed class Conversation : AggregateRoot
{
    public Guid UserAId { get; private set; }
    public Guid UserBId { get; private set; }

    private Conversation() { }

    private Conversation(Guid userId, Guid otherUserId) : base(Guid.NewGuid())
    {
        // The pair is stored in a fixed order, so one pair of users has at most one conversation (P3).
        (UserAId, UserBId) = userId.CompareTo(otherUserId) < 0 ? (userId, otherUserId) : (otherUserId, userId);
    }

    // Whether the starter follows the other user comes from Identity, through the module's service.
    public static Conversation Start(Guid starterId, Guid otherUserId, bool starterFollowsOther)
    {
        if (starterId == otherUserId)
        {
            throw new DomainException("A conversation needs two different users.");
        }
        if (!starterFollowsOther)
        {
            throw new DomainException("You can start a conversation only with a user you follow.");
        }

        return new Conversation(starterId, otherUserId);
    }

    // The conversation's chat is found by this context; the kind is local to the Social module.
    public ChatContext ChatContext => new("conversation", Id);

    public bool Includes(Guid userId)
    {
        return UserAId == userId || UserBId == userId;
    }

    public Guid OtherUser(Guid userId)
    {
        EnsureIncludes(userId);
        return UserAId == userId ? UserBId : UserAId;
    }

    // Only the two users take part, and neither is privileged (P4, P5).
    public ChatParticipant ChatParticipantFor(Guid userId)
    {
        EnsureIncludes(userId);
        return new ChatParticipant(userId, false);
    }

    private void EnsureIncludes(Guid userId)
    {
        if (!Includes(userId))
        {
            throw new DomainException("Only the two users of a conversation take part in it.");
        }
    }
}
