namespace Exploration.Application.TourChat;

public sealed record ChatStateDto(Guid Id, bool IsLocked, List<Guid> MutedUserIds);
