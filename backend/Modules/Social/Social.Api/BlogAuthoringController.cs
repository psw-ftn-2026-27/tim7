using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Images;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;

namespace Social.Api;

[ApiController]
[Route("api/social/blogs")]
[Authorize(Roles = "explorer")]
public sealed class BlogAuthoringController : ControllerBase
{
    private readonly BlogAuthoringService _authoringService;
    private readonly BlogAuthoringQueries _authoringQueries;

    public BlogAuthoringController(
        BlogAuthoringService authoringService,
        BlogAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    // Multipart form: the files cannot travel in JSON, so the action binds the form fields and builds the DTO from them.
    [HttpPost]
    [RequestSizeLimit(ImageUploadLimits.MaxRequestBytes)]
    public async Task<ActionResult<BlogDto>> Create(
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] List<IFormFile> images)
    {
        var dto = new CreateBlogDto(title, description, await ReadImagesAsync(images));
        return await _authoringService.CreateAsync(User.GetUserId(), dto);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _authoringService.PublishAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult> Archive(Guid id)
    {
        await _authoringService.ArchiveAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [RequestSizeLimit(ImageUploadLimits.MaxRequestBytes)]
    public async Task<ActionResult> Edit(
        Guid id,
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] List<string> keptImages,
        [FromForm] List<IFormFile> newImages)
    {
        var dto = new EditBlogDto(title, description, keptImages, await ReadImagesAsync(newImages));
        await _authoringService.EditAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<BlogDto>>> GetMine()
    {
        return await _authoringQueries.GetByAuthorAsync(User.GetUserId());
    }

    [HttpGet("mine/{id:guid}")]
    public async Task<ActionResult<BlogDto>> GetMineById(Guid id)
    {
        return await _authoringQueries.GetOwnedByIdAsync(id, User.GetUserId());
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
