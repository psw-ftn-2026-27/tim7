namespace Social.Application.Blogs;

// UserEmail comes from Identity and is filled in by the reading query; it stays empty elsewhere.
public sealed record CommentDto(Guid Id, Guid UserId, string Text, DateTime CreatedAt, DateTime? LastModifiedAt, string? UserEmail);
