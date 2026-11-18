using Shared.Domain.Exceptions;

namespace Shared.Domain.Chats;

public sealed class Chat : AggregateRoot
{
    private readonly List<MutedUser> _mutedUsers = [];

    public ChatContext Context { get; private set; }
    public bool IsLocked { get; private set; }
    public IReadOnlyList<MutedUser> MutedUsers => _mutedUsers;

    private Chat()
    {
        Context = null!;
    }

    private Chat(ChatContext context)
        : base(Guid.NewGuid())
    {
        Context = context;
        IsLocked = false;
    }

    public static Chat Open(ChatContext context)
    {
        return new Chat(context);
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public bool IsMuted(Guid userId)
    {
        return _mutedUsers.Any(muted => muted.UserId == userId);
    }

    public Message Write(ChatParticipant participant, string text, DateTime sentAt)
    {
        if (IsLocked)
        {
            throw new DomainException("The chat is locked.");
        }
        // A privileged participant is never muted, even before the embedding module forgets the mute.
        if (!participant.IsPrivileged && IsMuted(participant.UserId))
        {
            throw new DomainException("You are muted in this chat.");
        }

        return new Message(Id, participant.UserId, text, sentAt);
    }

    public void Mute(ChatParticipant by, ChatParticipant target)
    {
        EnsurePrivileged(by);
        if (by.UserId == target.UserId)
        {
            throw new DomainException("You cannot mute yourself.");
        }
        if (target.IsPrivileged)
        {
            throw new DomainException("A privileged participant cannot be muted.");
        }
        if (IsMuted(target.UserId))
        {
            throw new DomainException("This participant is already muted.");
        }

        _mutedUsers.Add(new MutedUser(target.UserId));
    }

    public void Unmute(ChatParticipant by, Guid userId)
    {
        EnsurePrivileged(by);
        var muted = _mutedUsers.SingleOrDefault(muted => muted.UserId == userId)
            ?? throw new DomainException("This user is not muted.");

        _mutedUsers.Remove(muted);
    }

    // Called by the embedding module when the user became privileged or stopped taking part.
    public void ForgetMute(Guid userId)
    {
        _mutedUsers.RemoveAll(muted => muted.UserId == userId);
    }

    private static void EnsurePrivileged(ChatParticipant participant)
    {
        if (!participant.IsPrivileged)
        {
            throw new DomainException("Only a privileged participant can moderate the chat.");
        }
    }
}
