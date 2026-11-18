namespace Social.Application.ClubChat;

public sealed record ChatMessageDto(
    Guid Id,
    Guid AuthorId,
    string? AuthorEmail,
    string Text,
    DateTime SentAt,
    bool AuthorCanBeMuted);
