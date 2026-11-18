using Exploration.Application.Chats;
using Exploration.Application.Tours;
using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourChat;

public sealed class TourChatQueries
{
    private readonly ITourRepository _tourRepository;
    private readonly IChatReadRepository _chatReadRepository;
    private readonly IMessageReadRepository _messageReadRepository;
    private readonly IIdentityApi _identityApi;

    public TourChatQueries(
        ITourRepository tourRepository,
        IChatReadRepository chatReadRepository,
        IMessageReadRepository messageReadRepository,
        IIdentityApi identityApi)
    {
        _tourRepository = tourRepository;
        _chatReadRepository = chatReadRepository;
        _messageReadRepository = messageReadRepository;
        _identityApi = identityApi;
    }

    public async Task<TourChatDto> GetAsync(Guid tourId, Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var tour = await _tourRepository.GetByIdAsync(tourId)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");
        var viewer = tour.ChatParticipantFor(userId);
        var chat = await _chatReadRepository.GetByContextAsync(tour.ChatContext);
        if (chat is null)
        {
            // Without a chat yet, the tour still says whether writing is possible.
            return new TourChatDto(new PageResult<ChatMessageDto>([], 0), viewer.IsPrivileged, false, tour.ChatIsLocked, []);
        }

        var messages = await _messageReadRepository.GetNewestAsync(chat.Id, page, pageSize);
        var users = await _identityApi.GetUsersByIdsAsync(
            messages.Items.Select(message => message.AuthorId).Concat(chat.MutedUserIds).Distinct().ToList());
        string? EmailOf(Guid id) => users.FirstOrDefault(user => user.Id == id)?.Email;

        var items = messages.Items
            .Select(message => message with
            {
                AuthorEmail = EmailOf(message.AuthorId),
                AuthorCanBeMuted = tour.CanBeMutedInChat(message.AuthorId) && !chat.MutedUserIds.Contains(message.AuthorId)
            })
            .ToList();
        var mutedUsers = viewer.IsPrivileged
            ? chat.MutedUserIds.Select(id => new ChatMutedUserDto(id, EmailOf(id))).ToList()
            : [];

        return new TourChatDto(
            messages with { Items = items },
            viewer.IsPrivileged,
            !viewer.IsPrivileged && chat.MutedUserIds.Contains(userId),
            chat.IsLocked,
            mutedUsers);
    }
}
