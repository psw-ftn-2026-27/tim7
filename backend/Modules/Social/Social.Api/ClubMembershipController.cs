using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubMembership;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubMembershipController : ControllerBase
{
    private readonly ClubMembershipService _membershipService;
    private readonly ClubMembershipQueries _membershipQueries;

    public ClubMembershipController(
        ClubMembershipService membershipService,
        ClubMembershipQueries membershipQueries)
    {
        _membershipService = membershipService;
        _membershipQueries = membershipQueries;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClubMembershipDto>> GetById(Guid id)
    {
        return await _membershipQueries.GetByIdAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/join-requests")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RequestToJoin(Guid id)
    {
        await _membershipService.RequestToJoinAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("{id:guid}/join-requests/mine")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> WithdrawRequest(Guid id)
    {
        await _membershipService.WithdrawRequestAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/join-requests/{userId:guid}/accept")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> AcceptRequest(Guid id, Guid userId)
    {
        await _membershipService.AcceptRequestAsync(id, userId, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/join-requests/{userId:guid}/reject")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RejectRequest(Guid id, Guid userId)
    {
        await _membershipService.RejectRequestAsync(id, userId, User.GetUserId());
        return NoContent();
    }
}
