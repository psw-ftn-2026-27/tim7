using Social.Application.ClubManagement;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<ClubDto?> GetByIdAsync(Guid id);
    Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId);
}
