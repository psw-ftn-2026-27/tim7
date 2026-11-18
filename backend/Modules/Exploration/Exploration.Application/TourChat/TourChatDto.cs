using Shared.Domain;

namespace Exploration.Application.TourChat;

public sealed record TourChatDto(
    PageResult<ChatMessageDto> Messages,
    bool IsPrivileged,
    bool IsMuted,
    bool IsLocked,
    List<ChatMutedUserDto> MutedUsers);
