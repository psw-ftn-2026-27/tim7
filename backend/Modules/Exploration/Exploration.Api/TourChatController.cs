using Exploration.Application.TourChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours/{tourId:guid}/chat")]
// Every signed-in user takes part in a tour's chat, whatever their role (P2).
[Authorize]
public sealed class TourChatController : ControllerBase
{
    private readonly TourChatService _chatService;
    private readonly TourChatQueries _chatQueries;

    public TourChatController(TourChatService chatService, TourChatQueries chatQueries)
    {
        _chatService = chatService;
        _chatQueries = chatQueries;
    }

    [HttpGet]
    public async Task<ActionResult<TourChatDto>> Get(Guid tourId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        return await _chatQueries.GetAsync(tourId, User.GetUserId(), page, pageSize);
    }

    [HttpPost("messages")]
    public async Task<ActionResult<Guid>> Send(Guid tourId, SendMessageDto dto)
    {
        return await _chatService.SendAsync(tourId, User.GetUserId(), dto);
    }

    [HttpDelete("messages/{messageId:guid}")]
    public async Task<ActionResult> DeleteMessage(Guid tourId, Guid messageId)
    {
        await _chatService.DeleteMessageAsync(tourId, messageId, User.GetUserId());
        return NoContent();
    }

    [HttpPost("muted/{userId:guid}")]
    public async Task<ActionResult> Mute(Guid tourId, Guid userId)
    {
        await _chatService.MuteAsync(tourId, userId, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("muted/{userId:guid}")]
    public async Task<ActionResult> Unmute(Guid tourId, Guid userId)
    {
        await _chatService.UnmuteAsync(tourId, userId, User.GetUserId());
        return NoContent();
    }
}
