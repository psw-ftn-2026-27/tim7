using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengeAuthoringController : ControllerBase
{
    private readonly SocialChallengeAuthoringService _authoringService;
    private readonly SocialChallengeAuthoringQueries _authoringQueries;

    public SocialChallengeAuthoringController(
        SocialChallengeAuthoringService authoringService,
        SocialChallengeAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateSocialChallengeDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateSocialChallengeDto dto)
    {
        await _authoringService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _authoringService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<SocialChallengeDto>>> GetAll()
    {
        return await _authoringQueries.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SocialChallengeDto>> GetById(Guid id)
    {
        return await _authoringQueries.GetByIdAsync(id);
    }
}
