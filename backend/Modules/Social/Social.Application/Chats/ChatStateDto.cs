namespace Social.Application.Chats;

public sealed record ChatStateDto(Guid Id, bool IsLocked, List<Guid> MutedUserIds);
