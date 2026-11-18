namespace Social.Domain.Blogs;

public sealed record BlogVote
{
    public Guid UserId { get; private init; }
    public BlogVoteType Type { get; private init; }
    public DateTime CastAt { get; private init; }

    private BlogVote() { }

    public BlogVote(Guid userId, BlogVoteType type)
    {
        UserId = userId;
        Type = type;
        CastAt = DateTime.UtcNow;
    }
}
