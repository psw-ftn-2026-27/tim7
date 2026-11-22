using Exploration.Application.Chats;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Shared.Domain.Chats;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourChat;

public sealed class TourChatService
{
    private readonly ITourRepository _tourRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourChatService(
        ITourRepository tourRepository,
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SendAsync(Guid tourId, Guid userId, SendMessageDto dto)
    {
        var tour = await GetTourAsync(tourId);
        var participant = tour.ChatParticipantFor(userId);
        var chat = await _chatRepository.GetByContextAsync(tour.ChatContext);
        if (chat is null)
        {
            chat = tour.OpenChat();
            _chatRepository.Add(chat);
        }

        var message = chat.Write(participant, dto.Text, DateTime.UtcNow);
        _messageRepository.Add(message);
        await _unitOfWork.SaveChangesAsync();
        return message.Id;
    }

    public async Task DeleteMessageAsync(Guid tourId, Guid messageId, Guid userId)
    {
        var tour = await GetTourAsync(tourId);
        var participant = tour.ChatParticipantFor(userId);
        var chat = await GetChatAsync(tour);
        // Looking the message up within this tour's chat keeps an author out of other tours' chats.
        var message = await _messageRepository.GetInChatAsync(chat.Id, messageId)
            ?? throw new NotFoundException($"Message {messageId} does not exist in this tour's chat.");

        message.DeleteBy(participant);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MuteAsync(Guid tourId, Guid targetId, Guid userId)
    {
        var tour = await GetTourAsync(tourId);
        var author = tour.ChatParticipantFor(userId);
        var target = tour.ChatParticipantFor(targetId);
        var chat = await GetChatAsync(tour);

        chat.Mute(author, target);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnmuteAsync(Guid tourId, Guid targetId, Guid userId)
    {
        var tour = await GetTourAsync(tourId);
        var author = tour.ChatParticipantFor(userId);
        var chat = await GetChatAsync(tour);

        chat.Unmute(author, targetId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Tour> GetTourAsync(Guid tourId)
    {
        return await _tourRepository.GetByIdAsync(tourId)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");
    }

    private async Task<Chat> GetChatAsync(Tour tour)
    {
        return await _chatRepository.GetByContextAsync(tour.ChatContext)
            ?? throw new NotFoundException($"Tour {tour.Id} has no chat yet.");
    }
}
