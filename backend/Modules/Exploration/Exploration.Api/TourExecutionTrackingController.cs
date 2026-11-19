using Exploration.Application.TourExecutionTracking;
using Exploration.Application.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tour-executions")]
[Authorize(Roles = "explorer")]
public sealed class TourExecutionTrackingController : ControllerBase
{
    private readonly TourExecutionTrackingService _trackingService;
    private readonly TourExecutionTrackingQueries _trackingQueries;

    public TourExecutionTrackingController(
        TourExecutionTrackingService trackingService,
        TourExecutionTrackingQueries trackingQueries)
    {
        _trackingService = trackingService;
        _trackingQueries = trackingQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> StartTour(StartTourDto dto)
    {
        return await _trackingService.StartTourAsync(User.GetUserId(), dto);
    }

    [HttpPost("active/abandon")]
    public async Task<ActionResult> Abandon()
    {
        await _trackingService.AbandonAsync(User.GetUserId());
        return NoContent();
    }

    [HttpPost("active/position-checks")]
    public async Task<ActionResult> CheckPosition(LocationDto position)
    {
        await _trackingService.CheckPositionAsync(User.GetUserId(), position);
        return NoContent();
    }

    // The session comes wrapped in MyActiveTourExecutionDto: ASP.NET answers a bare null with
    // 204 and no body, while "no active tour" is an ordinary state that should be a 200.
    [HttpGet("active")]
    public async Task<ActionResult<MyActiveTourExecutionDto>> GetActive()
    {
        return await _trackingQueries.GetActiveAsync(User.GetUserId());
    }

    [HttpGet("discovered-secrets/{tourId:guid}")]
    public async Task<ActionResult<List<DiscoveredSecretDto>>> GetDiscoveredSecrets(Guid tourId)
    {
        return await _trackingQueries.GetDiscoveredSecretsAsync(tourId, User.GetUserId());
    }
}
