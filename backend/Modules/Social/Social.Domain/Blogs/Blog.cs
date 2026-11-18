using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Blogs;

public sealed class Blog : AggregateRoot
{
    public Guid AuthorId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<string> Images { get; private set; }
    public BlogStatus Status { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    private readonly List<Comment> _comments = [];
    public IReadOnlyList<Comment> Comments => _comments;
    private readonly List<BlogVote> _votes = [];
    public IReadOnlyList<BlogVote> Votes => _votes;

    private Blog()
    {
        Title = null!;
        Description = null!;
        Images = null!;
    }

    public Blog(Guid authorId, string title, string description, List<string>? images)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A blog requires a title.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A blog requires a description.");
        }

        AuthorId = authorId;
        Title = title;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        Images = images ?? [];
        Status = BlogStatus.Draft;
    }

    public void Publish()
    {
        if (Status != BlogStatus.Draft)
        {
            throw new DomainException("Only a draft blog can be published.");
        }

        Status = BlogStatus.Published;
    }

    public void Archive()
    {
        if (Status != BlogStatus.Published)
        {
            throw new DomainException("Only a published blog can be archived.");
        }

        Status = BlogStatus.Archived;
    }

    // keptImages are addresses the blog already has; addedImages are addresses of newly uploaded images.
    public void Edit(string title, string description, List<string> keptImages, List<string> addedImages)
    {
        EnsureEditable();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A blog requires a title.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A blog requires a description.");
        }
        if (keptImages.Any(image => !Images.Contains(image)))
        {
            throw new DomainException("Only images the blog already has can be kept.");
        }

        Title = title;
        Description = description;
        Images = [.. keptImages, .. addedImages];
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddComment(Guid userId, string text)
    {
        EnsurePublished();

        _comments.Add(new Comment(userId, text));
    }

    public void UpdateComment(Guid commentId, Guid userId, string text)
    {
        EnsurePublished();

        GetOwnComment(commentId, userId).Update(text);
    }

    public void DeleteComment(Guid commentId, Guid userId)
    {
        EnsurePublished();

        _comments.Remove(GetOwnComment(commentId, userId));
    }

    public void Vote(Guid userId, BlogVoteType type)
    {
        if (Status != BlogStatus.Published)
        {
            throw new DomainException("Votes are allowed only on a published blog.");
        }

        var existing = _votes.FirstOrDefault(vote => vote.UserId == userId);
        if (existing is null)
        {
            _votes.Add(new BlogVote(userId, type));
            return;
        }

        _votes.Remove(existing);
        if (existing.Type != type)
        {
            _votes.Add(new BlogVote(userId, type));
        }
    }

    private void EnsurePublished()
    {
        if (Status != BlogStatus.Published)
        {
            throw new DomainException("Comments are allowed only on a published blog.");
        }
    }

    private void EnsureEditable()
    {
        if (Status != BlogStatus.Draft && Status != BlogStatus.Published)
        {
            throw new DomainException("Only a draft or published blog can be edited.");
        }
    }

    private Comment GetOwnComment(Guid commentId, Guid userId)
    {
        var comment = _comments.FirstOrDefault(comment => comment.Id == commentId && comment.UserId == userId);
        if (comment is null)
        {
            throw new NotFoundException($"Comment {commentId} does not exist.");
        }
        return comment;
    }
}
