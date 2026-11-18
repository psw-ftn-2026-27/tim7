using Shared.Domain.Exceptions;

namespace Shared.Domain.Chats;

public sealed class Chat : AggregateRoot
{
    public ChatContext Context { get; private set; }
    public bool IsLocked { get; private set; }

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

    public Message Write(ChatParticipant participant, string text, DateTime sentAt)
    {
        if (IsLocked)
        {
            throw new DomainException("The chat is locked.");
        }

        return new Message(Id, participant.UserId, text, sentAt);
    }
}
