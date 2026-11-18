using Exploration.Application.LocationSearches;
using Exploration.Domain.LocationSearches;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingService
{
    private readonly ILocationSearchRepository _locationSearchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourBrowsingService(ILocationSearchRepository locationSearchRepository, IUnitOfWork unitOfWork)
    {
        _locationSearchRepository = locationSearchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RecordLocationSearchAsync(RecordLocationSearchDto dto)
    {
        var locationSearch = new LocationSearch(
            new Location(dto.Center.Latitude, dto.Center.Longitude),
            new SearchRadius(dto.RadiusKilometers),
            dto.Name,
            dto.Difficulties,
            dto.Tags,
            dto.Transport,
            dto.MinPrice,
            dto.MaxPrice,
            dto.MinLength,
            dto.MaxLength,
            dto.ResultCount);

        _locationSearchRepository.Add(locationSearch);
        await _unitOfWork.SaveChangesAsync();
    }
}
