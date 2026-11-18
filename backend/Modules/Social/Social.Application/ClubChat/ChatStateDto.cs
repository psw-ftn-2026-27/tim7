namespace Social.Application.ClubChat;

public sealed record ChatStateDto(Guid Id, bool IsLocked, List<Guid> MutedUserIds);
