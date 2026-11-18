using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubRoles;

public sealed class ClubRolesService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubRolesService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PromoteToModeratorAsync(Guid clubId, Guid userId, Guid promoterId)
    {
        var club = await GetClubAsync(clubId);

        club.PromoteToModerator(promoterId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task TransferOwnershipAsync(Guid clubId, Guid newOwnerId, Guid ownerId)
    {
        var club = await GetClubAsync(clubId);

        club.TransferOwnership(ownerId, newOwnerId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid clubId, Guid userId, Guid removerId)
    {
        var club = await GetClubAsync(clubId);

        club.RemoveMember(removerId, userId);
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
