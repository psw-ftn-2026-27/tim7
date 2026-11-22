using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Chats;
using Social.Application.Conversations;

namespace Social.Application.DirectMessaging;

public sealed class DirectMessagingQueries
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationReadRepository _conversationReadRepository;
    private readonly IChatReadRepository _chatReadRepository;
    private readonly IMessageReadRepository _messageReadRepository;
    private readonly IIdentityApi _identityApi;

    public DirectMessagingQueries(
        IConversationRepository conversationRepository,
        IConversationReadRepository conversationReadRepository,
        IChatReadRepository chatReadRepository,
        IMessageReadRepository messageReadRepository,
        IIdentityApi identityApi)
    {
        _conversationRepository = conversationRepository;
        _conversationReadRepository = conversationReadRepository;
        _chatReadRepository = chatReadRepository;
        _messageReadRepository = messageReadRepository;
        _identityApi = identityApi;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
    {
        var conversations = await _conversationReadRepository.GetForUserAsync(userId);
        var users = await _identityApi.GetUsersByIdsAsync(conversations.Select(conversation => conversation.OtherUserId).ToList());

        return conversations
            .Select(conversation => conversation with
            {
                OtherUserEmail = users.FirstOrDefault(user => user.Id == conversation.OtherUserId)?.Email
            })
            .ToList();
    }

    // The users a conversation can be started with (P2).
    public async Task<List<FollowedUserDto>> GetFollowedUsersAsync(Guid userId)
    {
        var followed = await _identityApi.GetFollowedUsersAsync(userId);
        return followed.Select(user => new FollowedUserDto(user.Id, user.Email)).ToList();
    }

    public async Task<ConversationChatDto> GetChatAsync(Guid conversationId, Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var conversation = await _conversationRepository.GetByIdAsync(conversationId)
            ?? throw new NotFoundException($"Conversation {conversationId} does not exist.");
        var otherUserId = conversation.OtherUser(userId);
        var chat = await _chatReadRepository.GetByContextAsync(conversation.ChatContext);
        var messages = chat is null
            ? new PageResult<ChatMessageDto>([], 0)
            : await _messageReadRepository.GetNewestAsync(chat.Id, page, pageSize);

        var users = await _identityApi.GetUsersByIdsAsync([userId, otherUserId]);
        string? EmailOf(Guid id) => users.FirstOrDefault(user => user.Id == id)?.Email;

        var items = messages.Items.Select(message => message with { AuthorEmail = EmailOf(message.AuthorId) }).ToList();
        return new ConversationChatDto(otherUserId, EmailOf(otherUserId), messages with { Items = items });
    }
}
