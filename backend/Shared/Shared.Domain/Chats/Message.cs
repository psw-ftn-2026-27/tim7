using Shared.Domain.Exceptions;

namespace Shared.Domain.Chats;

public sealed class Message : AggregateRoot
{
    public const int MaxTextLength = 1000;

    public Guid ChatId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Text { get; private set; }
    public DateTime SentAt { get; private set; }

    private Message()
    {
        Text = null!;
    }

    // Internal so that a message is created only through Chat.Write, which checks whether the chat accepts it.
    internal Message(Guid chatId, Guid authorId, string text, DateTime sentAt)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A message cannot be empty.");
        }
        if (text.Length > MaxTextLength)
        {
            throw new DomainException($"A message cannot be longer than {MaxTextLength} characters.");
        }

        ChatId = chatId;
        AuthorId = authorId;
        Text = text;
        SentAt = sentAt;
    }
}
