using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubRoles;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize(Roles = "explorer")]
public sealed class ClubRolesController : ControllerBase
{
    private readonly ClubRolesService _rolesService;

    public ClubRolesController(ClubRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpPost("{id:guid}/members/{userId:guid}/promote")]
    public async Task<ActionResult> PromoteToModerator(Guid id, Guid userId)
    {
        await _rolesService.PromoteToModeratorAsync(id, userId, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/members/{userId:guid}/transfer-ownership")]
    public async Task<ActionResult> TransferOwnership(Guid id, Guid userId)
    {
        await _rolesService.TransferOwnershipAsync(id, userId, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<ActionResult> RemoveMember(Guid id, Guid userId)
    {
        await _rolesService.RemoveMemberAsync(id, userId, User.GetUserId());
        return NoContent();
    }
}
