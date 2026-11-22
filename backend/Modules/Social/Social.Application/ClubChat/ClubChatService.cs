using Shared.Domain.Chats;
using Shared.Domain.Exceptions;
using Social.Application.Chats;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubChat;

public sealed class ClubChatService
{
    private readonly IClubRepository _clubRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubChatService(
        IClubRepository clubRepository,
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SendAsync(Guid clubId, Guid userId, SendMessageDto dto)
    {
        var club = await GetClubAsync(clubId);
        var participant = club.ChatParticipantFor(userId);
        var chat = await _chatRepository.GetByContextAsync(club.ChatContext);
        if (chat is null)
        {
            chat = Chat.Open(club.ChatContext);
            _chatRepository.Add(chat);
        }

        var message = chat.Write(participant, dto.Text, DateTime.UtcNow);
        _messageRepository.Add(message);
        await _unitOfWork.SaveChangesAsync();
        return message.Id;
    }

    public async Task DeleteMessageAsync(Guid clubId, Guid messageId, Guid userId)
    {
        var club = await GetClubAsync(clubId);
        var participant = club.ChatParticipantFor(userId);
        var chat = await GetChatAsync(club);
        // Looking the message up within this club's chat keeps a moderator out of other clubs' chats.
        var message = await _messageRepository.GetInChatAsync(chat.Id, messageId)
            ?? throw new NotFoundException($"Message {messageId} does not exist in this club's chat.");

        message.DeleteBy(participant);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MuteAsync(Guid clubId, Guid targetId, Guid userId)
    {
        var club = await GetClubAsync(clubId);
        var moderator = club.ChatParticipantFor(userId);
        var target = club.ChatParticipantFor(targetId);
        var chat = await GetChatAsync(club);

        chat.Mute(moderator, target);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnmuteAsync(Guid clubId, Guid targetId, Guid userId)
    {
        var club = await GetClubAsync(clubId);
        var moderator = club.ChatParticipantFor(userId);
        var chat = await GetChatAsync(club);

        chat.Unmute(moderator, targetId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        return await _clubRepository.GetByIdAsync(clubId)
            ?? throw new NotFoundException($"Club {clubId} does not exist.");
    }

    private async Task<Chat> GetChatAsync(Club club)
    {
        return await _chatRepository.GetByContextAsync(club.ChatContext)
            ?? throw new NotFoundException($"Club {club.Id} has no chat yet.");
    }
}
