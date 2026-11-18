using Exploration.Application.KeyPointAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Images;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class KeyPointAuthoringController : ControllerBase
{
    private readonly KeyPointAuthoringService _authoringService;
    private readonly KeyPointAuthoringQueries _authoringQueries;

    public KeyPointAuthoringController(
        KeyPointAuthoringService authoringService,
        KeyPointAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet("mine/{id:guid}")]
    public async Task<ActionResult<AuthorTourDto>> GetMine(Guid id)
    {
        return await _authoringQueries.GetAuthorTourAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/key-points")]
    [RequestSizeLimit(ImageUploadLimits.MaxRequestBytes)]
    public async Task<ActionResult> AddKeyPoint(Guid id, [FromForm] SaveKeyPointDto dto, IFormFile image)
    {
        var uploaded = await ImageUploadReader.ReadAsync(image);
        var imageDto = new ImageUploadDto(uploaded.FileName, uploaded.Content, uploaded.Width, uploaded.Height);

        await _authoringService.AddKeyPointAsync(id, User.GetUserId(), dto, imageDto);
        return NoContent();
    }

    // The image is optional here: without a new file the key point keeps its current image.
    [HttpPut("{id:guid}/key-points/{keyPointId:guid}")]
    [RequestSizeLimit(ImageUploadLimits.MaxRequestBytes)]
    public async Task<ActionResult> ChangeKeyPoint(Guid id, Guid keyPointId, [FromForm] SaveKeyPointDto dto, IFormFile? image)
    {
        ImageUploadDto? imageDto = null;
        if (image is not null)
        {
            var uploaded = await ImageUploadReader.ReadAsync(image);
            imageDto = new ImageUploadDto(uploaded.FileName, uploaded.Content, uploaded.Width, uploaded.Height);
        }

        await _authoringService.ChangeKeyPointAsync(id, User.GetUserId(), keyPointId, dto, imageDto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> RemoveKeyPoint(Guid id, Guid keyPointId)
    {
        await _authoringService.RemoveKeyPointAsync(id, User.GetUserId(), keyPointId);
        return NoContent();
    }
}
