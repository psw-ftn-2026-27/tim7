using Identity.Contracts;
using Shared.Domain.Chats;
using Shared.Domain.Exceptions;
using Social.Application.Chats;
using Social.Application.Conversations;
using Social.Domain.Conversations;

namespace Social.Application.DirectMessaging;

public sealed class DirectMessagingService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IIdentityApi _identityApi;
    private readonly IUnitOfWork _unitOfWork;

    public DirectMessagingService(
        IConversationRepository conversationRepository,
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IIdentityApi identityApi,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _identityApi = identityApi;
        _unitOfWork = unitOfWork;
    }

    // Returns the conversation of the pair: the existing one, or a new one (P3).
    public async Task<Guid> StartAsync(Guid userId, StartConversationDto dto)
    {
        var existing = await _conversationRepository.GetBetweenAsync(userId, dto.OtherUserId);
        if (existing is not null)
        {
            return existing.Id;
        }

        var followed = await _identityApi.GetFollowedUsersAsync(userId);
        var conversation = Conversation.Start(userId, dto.OtherUserId, followed.Any(user => user.Id == dto.OtherUserId));

        _conversationRepository.Add(conversation);
        await _unitOfWork.SaveChangesAsync();
        return conversation.Id;
    }

    public async Task<Guid> SendAsync(Guid conversationId, Guid userId, SendMessageDto dto)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId)
            ?? throw new NotFoundException($"Conversation {conversationId} does not exist.");
        var participant = conversation.ChatParticipantFor(userId);
        var chat = await _chatRepository.GetByContextAsync(conversation.ChatContext);
        if (chat is null)
        {
            chat = Chat.Open(conversation.ChatContext);
            _chatRepository.Add(chat);
        }

        var message = chat.Write(participant, dto.Text, DateTime.UtcNow);
        _messageRepository.Add(message);
        await _unitOfWork.SaveChangesAsync();
        return message.Id;
    }
}
