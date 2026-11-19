using Games.Application.LocationChallengePlaying;
using Games.Application.LocationChallenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges/playing")]
[Authorize(Roles = "explorer")]
public sealed class LocationChallengePlayingController : ControllerBase
{
    private readonly LocationChallengePlayingService _playingService;
    private readonly LocationChallengePlayingQueries _playingQueries;

    public LocationChallengePlayingController(
        LocationChallengePlayingService playingService,
        LocationChallengePlayingQueries playingQueries)
    {
        _playingService = playingService;
        _playingQueries = playingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlayableLocationChallengeDto>>> GetAll()
    {
        return await _playingQueries.GetLocationChallengesAsync(User.GetUserId());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlayedLocationChallengeDto>> Get(Guid id)
    {
        return await _playingQueries.GetLocationChallengeAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id, [FromBody] LocationDto position)
    {
        await _playingService.ActivateLocationChallengeAsync(User.GetUserId(), id, position);
        return NoContent();
    }

    [HttpPost("{id:guid}/check")]
    public async Task<ActionResult> Check(Guid id, [FromBody] LocationDto position)
    {
        await _playingService.CheckHiddenPointAsync(User.GetUserId(), id, position);
        return NoContent();
    }
}
