using Shared.Domain;
using Social.Application.Clubs;

namespace Social.Application.ClubBrowsing;

public sealed class ClubBrowsingQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubBrowsingQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _clubReadRepository.GetAllAsync(page, pageSize);
    }
}
