using Identity.Core.PositionSimulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Identity.Api;

[ApiController]
[Route("api/identity/position")]
[Authorize]
public sealed class PositionSimulationController : ControllerBase
{
    private readonly PositionSimulationService _positionSimulationService;
    private readonly PositionSimulationQueries _positionSimulationQueries;

    public PositionSimulationController(PositionSimulationService positionSimulationService,
        PositionSimulationQueries positionSimulationQueries)
    {
        _positionSimulationService = positionSimulationService;
        _positionSimulationQueries = positionSimulationQueries;
    }

    // A user who has never marked a position gets null, which ASP.NET answers with 204 and no body.
    [HttpGet]
    public async Task<ActionResult<PositionDto?>> GetMyPosition()
    {
        return await _positionSimulationQueries.GetMyPositionAsync(User.GetUserId());
    }

    [HttpPut]
    public async Task<ActionResult> SetPosition(SetPositionDto dto)
    {
        await _positionSimulationService.SetPositionAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
