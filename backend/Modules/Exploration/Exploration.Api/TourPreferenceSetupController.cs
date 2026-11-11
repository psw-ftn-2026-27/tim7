using Exploration.Application.TourPreferenceSetup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tour-preferences")]
[Authorize(Roles = "explorer")]
public sealed class TourPreferenceSetupController : ControllerBase
{
    private readonly TourPreferenceSetupService _setupService;
    private readonly TourPreferenceSetupQueries _setupQueries;

    public TourPreferenceSetupController(
        TourPreferenceSetupService setupService,
        TourPreferenceSetupQueries setupQueries)
    {
        _setupService = setupService;
        _setupQueries = setupQueries;
    }

    // The preferences come wrapped in MyTourPreferencesDto: ASP.NET answers a bare null with
    // 204 and no body, while "no preferences yet" is an ordinary state that should be a 200.
    [HttpGet("mine")]
    public async Task<ActionResult<MyTourPreferencesDto>> GetMine()
    {
        return await _setupQueries.GetMineAsync(User.GetUserId());
    }

    [HttpPut("mine")]
    public async Task<ActionResult<TourPreferenceDto>> SaveMine(SaveTourPreferenceDto dto)
    {
        return await _setupService.SaveMineAsync(User.GetUserId(), dto);
    }

    [HttpDelete("mine")]
    public async Task<ActionResult> DeleteMine()
    {
        await _setupService.DeleteMineAsync(User.GetUserId());
        return NoContent();
    }

    [HttpGet("available-tags")]
    public async Task<ActionResult<List<string>>> GetAvailableTags()
    {
        return await _setupQueries.GetAvailableTagsAsync();
    }
}
