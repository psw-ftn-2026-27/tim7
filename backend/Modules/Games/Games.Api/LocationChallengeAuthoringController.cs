using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Images;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class LocationChallengeAuthoringController : ControllerBase
{
    private readonly LocationChallengeAuthoringService _authoringService;
    private readonly LocationChallengeAuthoringQueries _authoringQueries;

    public LocationChallengeAuthoringController(
        LocationChallengeAuthoringService authoringService,
        LocationChallengeAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    // Multipart form: the DTO's fields and the image files travel side by side.
    [HttpPost]
    [RequestSizeLimit(ImageUploadLimits.MaxRequestBytes)]
    public async Task<ActionResult<Guid>> Create([FromForm] CreateLocationChallengeDto dto, List<IFormFile> images)
    {
        return await _authoringService.CreateAsync(dto, await ReadImagesAsync(images));
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationChallengeDto>>> GetAll()
    {
        return await _authoringQueries.GetAllAsync();
    }

    // Part of binding the request: each uploaded file is read and copied into the module's own DTO.
    private static async Task<List<ImageUploadDto>> ReadImagesAsync(List<IFormFile> files)
    {
        var images = new List<ImageUploadDto>();
        foreach (var file in files)
        {
            var uploaded = await ImageUploadReader.ReadAsync(file);
            images.Add(new ImageUploadDto(uploaded.FileName, uploaded.Content, uploaded.Width, uploaded.Height));
        }
        return images;
    }
}
