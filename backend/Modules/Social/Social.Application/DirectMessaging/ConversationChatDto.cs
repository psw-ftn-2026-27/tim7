using Shared.Domain;
using Social.Application.Chats;

namespace Social.Application.DirectMessaging;

public sealed record ConversationChatDto(Guid OtherUserId, string? OtherUserEmail, PageResult<ChatMessageDto> Messages);
