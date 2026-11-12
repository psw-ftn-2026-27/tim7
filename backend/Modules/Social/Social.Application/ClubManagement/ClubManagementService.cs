using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubManagement;

public sealed class ClubManagementService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubManagementService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(Guid ownerId, CreateClubDto dto)
    {
        var club = new Club(ownerId, dto.Name, dto.Description, dto.Images);

        _clubRepository.Add(club);
        await _unitOfWork.SaveChangesAsync();

        return club.Id;
    }

    public async Task UpdateAsync(Guid clubId, Guid ownerId, UpdateClubDto dto)
    {
        var club = await GetOwnedClubAsync(clubId, ownerId);

        club.Update(dto.Name, dto.Description, dto.Images);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid clubId, Guid ownerId)
    {
        var club = await GetOwnedClubAsync(clubId, ownerId);

        _clubRepository.Remove(club);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetOwnedClubAsync(Guid clubId, Guid ownerId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null || club.OwnerId != ownerId)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}
