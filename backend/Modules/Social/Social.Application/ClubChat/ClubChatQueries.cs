using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Chats;
using Social.Application.Clubs;

namespace Social.Application.ClubChat;

public sealed class ClubChatQueries
{
    private readonly IClubRepository _clubRepository;
    private readonly IChatReadRepository _chatReadRepository;
    private readonly IMessageReadRepository _messageReadRepository;
    private readonly IIdentityApi _identityApi;

    public ClubChatQueries(
        IClubRepository clubRepository,
        IChatReadRepository chatReadRepository,
        IMessageReadRepository messageReadRepository,
        IIdentityApi identityApi)
    {
        _clubRepository = clubRepository;
        _chatReadRepository = chatReadRepository;
        _messageReadRepository = messageReadRepository;
        _identityApi = identityApi;
    }

    public async Task<ClubChatDto> GetAsync(Guid clubId, Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var club = await _clubRepository.GetByIdAsync(clubId)
            ?? throw new NotFoundException($"Club {clubId} does not exist.");
        var viewer = club.ChatParticipantFor(userId);
        var chat = await _chatReadRepository.GetByContextAsync(club.ChatContext);
        if (chat is null)
        {
            return new ClubChatDto(new PageResult<ChatMessageDto>([], 0), viewer.IsPrivileged, false, false, []);
        }

        var messages = await _messageReadRepository.GetNewestAsync(chat.Id, page, pageSize);
        var users = await _identityApi.GetUsersByIdsAsync(
            messages.Items.Select(message => message.AuthorId).Concat(chat.MutedUserIds).Distinct().ToList());
        string? EmailOf(Guid id) => users.FirstOrDefault(user => user.Id == id)?.Email;

        var items = messages.Items
            .Select(message => message with
            {
                AuthorEmail = EmailOf(message.AuthorId),
                AuthorCanBeMuted = club.CanBeMutedInChat(message.AuthorId) && !chat.MutedUserIds.Contains(message.AuthorId)
            })
            .ToList();
        var mutedUsers = viewer.IsPrivileged
            ? chat.MutedUserIds.Select(id => new ChatMutedUserDto(id, EmailOf(id))).ToList()
            : [];

        return new ClubChatDto(
            messages with { Items = items },
            viewer.IsPrivileged,
            !viewer.IsPrivileged && chat.MutedUserIds.Contains(userId),
            chat.IsLocked,
            mutedUsers);
    }
}
