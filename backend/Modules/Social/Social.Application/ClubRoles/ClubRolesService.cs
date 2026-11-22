using Shared.Domain.Exceptions;
using Social.Application.Chats;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubRoles;

public sealed class ClubRolesService
{
    private readonly IClubRepository _clubRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubRolesService(IClubRepository clubRepository, IChatRepository chatRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _chatRepository = chatRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PromoteToModeratorAsync(Guid clubId, Guid userId, Guid promoterId)
    {
        var club = await GetClubAsync(clubId);

        club.PromoteToModerator(promoterId, userId);
        await ForgetMuteAsync(club, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task TransferOwnershipAsync(Guid clubId, Guid newOwnerId, Guid ownerId)
    {
        var club = await GetClubAsync(clubId);

        club.TransferOwnership(ownerId, newOwnerId);
        await ForgetMuteAsync(club, newOwnerId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid clubId, Guid userId, Guid removerId)
    {
        var club = await GetClubAsync(clubId);

        club.RemoveMember(removerId, userId);
        await ForgetMuteAsync(club, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    // A user who became privileged or left the club is no longer muted in its chat (P7).
    private async Task ForgetMuteAsync(Club club, Guid userId)
    {
        var chat = await _chatRepository.GetByContextAsync(club.ChatContext);
        chat?.ForgetMute(userId);
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}
