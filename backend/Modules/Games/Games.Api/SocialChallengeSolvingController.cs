using Games.Application.LocationChallenges;
using Games.Application.SocialChallengeSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/social-challenges/solving")]
[Authorize(Roles = "explorer")]
public sealed class SocialChallengeSolvingController : ControllerBase
{
    private readonly SocialChallengeSolvingService _solvingService;
    private readonly SocialChallengeSolvingQueries _solvingQueries;

    public SocialChallengeSolvingController(
        SocialChallengeSolvingService solvingService,
        SocialChallengeSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<SolvableSocialChallengeDto>>> GetAll()
    {
        return await _solvingQueries.GetChallengesAsync(User.GetUserId());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SocialChallengeRunDto>> GetRun(Guid id)
    {
        return await _solvingQueries.GetRunAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id, [FromBody] LocationDto position)
    {
        await _solvingService.ActivateAsync(User.GetUserId(), id, position);
        return NoContent();
    }

    [HttpPost("{id:guid}/position")]
    public async Task<ActionResult> ReportPosition(Guid id, [FromBody] LocationDto position)
    {
        await _solvingService.ReportPositionAsync(User.GetUserId(), id, position);
        return NoContent();
    }
}
