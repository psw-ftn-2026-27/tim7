using Identity.Core.Connections;

namespace Identity.Core.Following;

public sealed record FollowRequestDto(Guid UserId, string Email, FollowRequestStatus Status);
