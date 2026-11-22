using Shared.Domain;

namespace Social.Application.ClubChat;

public sealed record ClubChatDto(
    PageResult<ChatMessageDto> Messages,
    bool IsPrivileged,
    bool IsMuted,
    bool IsLocked,
    List<ChatMutedUserDto> MutedUsers);
