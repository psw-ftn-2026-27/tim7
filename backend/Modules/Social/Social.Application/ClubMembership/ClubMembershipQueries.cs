using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubMembership;

public sealed class ClubMembershipQueries
{
    private readonly IClubReadRepository _clubReadRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IIdentityApi _identityApi;

    public ClubMembershipQueries(
        IClubReadRepository clubReadRepository,
        IClubRepository clubRepository,
        IIdentityApi identityApi)
    {
        _clubReadRepository = clubReadRepository;
        _clubRepository = clubRepository;
        _identityApi = identityApi;
    }

    public async Task<ClubMembershipDto> GetByIdAsync(Guid clubId, Guid userId)
    {
        var view = await _clubReadRepository.GetMembershipByIdAsync(clubId);
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (view is null || club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        var myState = club.IsMember(userId)
            ? MembershipState.Member
            : club.HasPendingRequest(userId) ? MembershipState.Pending : MembershipState.None;
        return view with
        {
            MyState = myState,
            MyRole = view.Members.FirstOrDefault(member => member.UserId == userId)?.Role,
            PendingRequests = club.CanResolveRequests(userId) ? view.PendingRequests : null,
            SentInvitations = club.OwnerId == userId ? await WithEmailsAsync(view.SentInvitations!) : null
        };
    }

    private async Task<List<ClubInvitationDto>> WithEmailsAsync(List<ClubInvitationDto> invitations)
    {
        var invitedUsers = await _identityApi.GetUsersByIdsAsync(
            invitations.Select(invitation => invitation.UserId).ToList());
        return invitations
            .Select(invitation => invitation with
            {
                UserEmail = invitedUsers.FirstOrDefault(user => user.Id == invitation.UserId)?.Email
            })
            .ToList();
    }
}
