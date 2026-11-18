using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubInvitations;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubInvitationsController : ControllerBase
{
    private readonly ClubInvitationsService _invitationsService;
    private readonly ClubInvitationsQueries _invitationsQueries;

    public ClubInvitationsController(
        ClubInvitationsService invitationsService,
        ClubInvitationsQueries invitationsQueries)
    {
        _invitationsService = invitationsService;
        _invitationsQueries = invitationsQueries;
    }

    [HttpGet("invitations")]
    public async Task<ActionResult<List<MyInvitationDto>>> GetMyInvitations()
    {
        return await _invitationsQueries.GetMyInvitationsAsync(User.GetUserId());
    }

    [HttpGet("{id:guid}/explorers")]
    public async Task<ActionResult<List<ExplorerDto>>> SearchExplorers(Guid id, [FromQuery] string username = "")
    {
        return await _invitationsQueries.SearchExplorersAsync(id, User.GetUserId(), username);
    }

    [HttpPost("{id:guid}/invitations/{userId:guid}")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> Invite(Guid id, Guid userId)
    {
        await _invitationsService.InviteAsync(id, User.GetUserId(), userId);
        return NoContent();
    }

    [HttpPost("{id:guid}/invitations/mine/accept")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> AcceptInvitation(Guid id)
    {
        await _invitationsService.AcceptInvitationAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/invitations/mine/reject")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RejectInvitation(Guid id)
    {
        await _invitationsService.RejectInvitationAsync(id, User.GetUserId());
        return NoContent();
    }
}
