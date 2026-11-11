namespace Identity.Core.Following;

public sealed record FollowRequestsDto(List<FollowRequestDto> Received, List<FollowRequestDto> Sent);
