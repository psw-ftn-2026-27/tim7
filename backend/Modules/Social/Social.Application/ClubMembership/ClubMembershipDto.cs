using Social.Domain.Clubs;

namespace Social.Application.ClubMembership;

public sealed record ClubMembershipDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    List<string> Images,
    List<ClubMemberDto> Members,
    MembershipState MyState,
    ClubRole? MyRole,
    List<Guid>? PendingRequests);
