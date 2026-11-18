using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.Chats;
using Social.Application.DirectMessaging;

namespace Social.Api;

[ApiController]
[Route("api/social/conversations")]
[Authorize(Roles = "explorer")]
public sealed class DirectMessagingController : ControllerBase
{
    private readonly DirectMessagingService _messagingService;
    private readonly DirectMessagingQueries _messagingQueries;

    public DirectMessagingController(DirectMessagingService messagingService, DirectMessagingQueries messagingQueries)
    {
        _messagingService = messagingService;
        _messagingQueries = messagingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations()
    {
        return await _messagingQueries.GetConversationsAsync(User.GetUserId());
    }

    [HttpGet("followed-users")]
    public async Task<ActionResult<List<FollowedUserDto>>> GetFollowedUsers()
    {
        return await _messagingQueries.GetFollowedUsersAsync(User.GetUserId());
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Start(StartConversationDto dto)
    {
        return await _messagingService.StartAsync(User.GetUserId(), dto);
    }

    [HttpGet("{id:guid}/chat")]
    public async Task<ActionResult<ConversationChatDto>> GetChat(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        return await _messagingQueries.GetChatAsync(id, User.GetUserId(), page, pageSize);
    }

    [HttpPost("{id:guid}/chat/messages")]
    public async Task<ActionResult<Guid>> Send(Guid id, SendMessageDto dto)
    {
        return await _messagingService.SendAsync(id, User.GetUserId(), dto);
    }
}
