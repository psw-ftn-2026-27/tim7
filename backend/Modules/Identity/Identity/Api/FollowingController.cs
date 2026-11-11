using Identity.Core.Following;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Identity.Api;

[ApiController]
[Route("api/identity")]
[Authorize(Roles = "explorer")]
public sealed class FollowingController : ControllerBase
{
    private readonly FollowingService _followingService;
    private readonly FollowingQueries _followingQueries;

    public FollowingController(FollowingService followingService, FollowingQueries followingQueries)
    {
        _followingService = followingService;
        _followingQueries = followingQueries;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PageResult<FoundUserDto>>> SearchUsers(
        [FromQuery] string email = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _followingQueries.SearchUsersAsync(User.GetUserId(), email, page, pageSize);
    }

    [HttpGet("requests")]
    public async Task<ActionResult<FollowRequestsDto>> GetRequests()
    {
        return await _followingQueries.GetRequestsAsync(User.GetUserId());
    }

    [HttpPost("requests/sent/{recipientId:guid}")]
    public async Task<ActionResult> SendRequest(Guid recipientId)
    {
        await _followingService.SendRequestAsync(User.GetUserId(), recipientId);
        return NoContent();
    }

    [HttpDelete("requests/sent/{recipientId:guid}")]
    public async Task<ActionResult> WithdrawRequest(Guid recipientId)
    {
        await _followingService.WithdrawRequestAsync(User.GetUserId(), recipientId);
        return NoContent();
    }

    [HttpPost("requests/received/{senderId:guid}/accept")]
    public async Task<ActionResult> AcceptRequest(Guid senderId)
    {
        await _followingService.AcceptRequestAsync(User.GetUserId(), senderId);
        return NoContent();
    }

    [HttpPost("requests/received/{senderId:guid}/reject")]
    public async Task<ActionResult> RejectRequest(Guid senderId)
    {
        await _followingService.RejectRequestAsync(User.GetUserId(), senderId);
        return NoContent();
    }

    [HttpGet("followers")]
    public async Task<ActionResult<PageResult<UserDto>>> GetFollowers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _followingQueries.GetFollowersAsync(User.GetUserId(), page, pageSize);
    }

    [HttpDelete("followers/{followerId:guid}")]
    public async Task<ActionResult> RemoveFollower(Guid followerId)
    {
        await _followingService.RemoveFollowerAsync(User.GetUserId(), followerId);
        return NoContent();
    }

    [HttpGet("following")]
    public async Task<ActionResult<PageResult<UserDto>>> GetFollowing([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _followingQueries.GetFollowingAsync(User.GetUserId(), page, pageSize);
    }

    [HttpDelete("following/{followedId:guid}")]
    public async Task<ActionResult> Unfollow(Guid followedId)
    {
        await _followingService.UnfollowAsync(User.GetUserId(), followedId);
        return NoContent();
    }
}
