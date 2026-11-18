using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubInvitations;

public sealed class ClubInvitationsService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public ClubInvitationsService(IClubRepository clubRepository, IUnitOfWork unitOfWork, IIdentityApi identityApi)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task InviteAsync(Guid clubId, Guid ownerId, Guid userId)
    {
        var club = await GetClubAsync(clubId);
        if (!await _identityApi.UserExistsAsync(userId))
        {
            throw new NotFoundException($"User {userId} does not exist.");
        }

        club.Invite(ownerId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptInvitationAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.AcceptInvitation(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectInvitationAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.RejectInvitation(userId);
        await _unitOfWork.SaveChangesAsync();
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
