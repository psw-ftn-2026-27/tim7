namespace Social.Application.DirectMessaging;

public sealed record ConversationDto(Guid Id, Guid OtherUserId, string? OtherUserEmail, DateTime? LastMessageAt);
