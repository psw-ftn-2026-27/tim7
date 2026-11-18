namespace Shared.Domain.Chats;

public sealed record ChatContext
{
    public string Kind { get; private init; }
    public Guid ContextId { get; private init; }

    private ChatContext()
    {
        Kind = null!;
    }

    public ChatContext(string kind, Guid contextId)
    {
        Kind = kind;
        ContextId = contextId;
    }
}
