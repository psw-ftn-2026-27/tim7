using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize]
public sealed class TourBrowsingController : ControllerBase
{
    private readonly TourBrowsingService _browsingService;
    private readonly TourBrowsingQueries _browsingQueries;

    public TourBrowsingController(TourBrowsingService browsingService, TourBrowsingQueries browsingQueries)
    {
        _browsingService = browsingService;
        _browsingQueries = browsingQueries;
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<TourDto>>> GetPublished([FromQuery] TourSearchDto search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _browsingQueries.GetPublishedAsync(search, page, pageSize);
    }

    [HttpGet("published/map")]
    public async Task<ActionResult<PageResult<TourOnMapDto>>> GetPublishedOnMap([FromQuery] TourSearchDto search,
        [FromQuery] double? latitude, [FromQuery] double? longitude, [FromQuery] double? radiusKilometers,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _browsingQueries.GetPublishedOnMapAsync(search, latitude, longitude, radiusKilometers, page, pageSize);
    }

    [HttpPost("location-searches")]
    public async Task<ActionResult> RecordLocationSearch(RecordLocationSearchDto dto)
    {
        await _browsingService.RecordLocationSearchAsync(dto);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublishedTourDto>> GetPublishedTour(Guid id)
    {
        return await _browsingQueries.GetPublishedTourAsync(id);
    }
}
