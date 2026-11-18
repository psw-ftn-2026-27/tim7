using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubManagement;

public sealed class ClubManagementQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubManagementQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public Task<List<ClubDto>> GetMineAsync(Guid ownerId)
    {
        return _clubReadRepository.GetByOwnerAsync(ownerId);
    }

    public async Task<ClubDto> GetOwnedByIdAsync(Guid clubId, Guid ownerId)
    {
        var club = await _clubReadRepository.GetByIdAsync(clubId);
        if (club is null || club.OwnerId != ownerId)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        return club;
    }
}
