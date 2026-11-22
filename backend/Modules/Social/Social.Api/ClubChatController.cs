using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubChat;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs/{clubId:guid}/chat")]
[Authorize(Roles = "explorer")]
public sealed class ClubChatController : ControllerBase
{
    private readonly ClubChatService _chatService;
    private readonly ClubChatQueries _chatQueries;

    public ClubChatController(ClubChatService chatService, ClubChatQueries chatQueries)
    {
        _chatService = chatService;
        _chatQueries = chatQueries;
    }

    [HttpGet]
    public async Task<ActionResult<ClubChatDto>> Get(Guid clubId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        return await _chatQueries.GetAsync(clubId, User.GetUserId(), page, pageSize);
    }

    [HttpPost("messages")]
    public async Task<ActionResult<Guid>> Send(Guid clubId, SendMessageDto dto)
    {
        return await _chatService.SendAsync(clubId, User.GetUserId(), dto);
    }

    [HttpDelete("messages/{messageId:guid}")]
    public async Task<ActionResult> DeleteMessage(Guid clubId, Guid messageId)
    {
        await _chatService.DeleteMessageAsync(clubId, messageId, User.GetUserId());
        return NoContent();
    }

    [HttpPost("muted/{userId:guid}")]
    public async Task<ActionResult> Mute(Guid clubId, Guid userId)
    {
        await _chatService.MuteAsync(clubId, userId, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("muted/{userId:guid}")]
    public async Task<ActionResult> Unmute(Guid clubId, Guid userId)
    {
        await _chatService.UnmuteAsync(clubId, userId, User.GetUserId());
        return NoContent();
    }
}
