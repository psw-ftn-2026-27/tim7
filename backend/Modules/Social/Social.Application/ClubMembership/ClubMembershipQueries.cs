using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubMembership;

public sealed class ClubMembershipQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubMembershipQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public async Task<ClubMembershipDto> GetByIdAsync(Guid clubId, Guid userId)
    {
        var club = await _clubReadRepository.GetMembershipByIdAsync(clubId, userId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        return club;
    }
}
