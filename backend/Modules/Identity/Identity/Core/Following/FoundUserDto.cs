namespace Identity.Core.Following;

public sealed record FoundUserDto(Guid Id, string Email, RelationshipStatus Status);
