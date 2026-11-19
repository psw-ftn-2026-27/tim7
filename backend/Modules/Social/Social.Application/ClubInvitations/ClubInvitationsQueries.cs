using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubInvitations;

public sealed class ClubInvitationsQueries
{
    private readonly IClubReadRepository _clubReadRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IIdentityApi _identityApi;

    public ClubInvitationsQueries(
        IClubReadRepository clubReadRepository,
        IClubRepository clubRepository,
        IIdentityApi identityApi)
    {
        _clubReadRepository = clubReadRepository;
        _clubRepository = clubRepository;
        _identityApi = identityApi;
    }

    // Only the owner may search, so that explorers' e-mails are not open to every signed-in user.
    // Members (the owner included) and users already invited are left out, because the club
    // would refuse to invite them.
    public async Task<List<ExplorerDto>> SearchExplorersAsync(Guid clubId, Guid userId, string username)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null || club.OwnerId != userId)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        var explorers = await _identityApi.SearchExplorersAsync(username);
        return explorers
            .Where(explorer => !club.IsMember(explorer.Id) && !club.HasPendingInvitation(explorer.Id))
            .Select(explorer => new ExplorerDto(explorer.Id, explorer.Email))
            .ToList();
    }

    public Task<List<MyInvitationDto>> GetMyInvitationsAsync(Guid userId)
    {
        return _clubReadRepository.GetInvitationsOfUserAsync(userId);
    }
}
