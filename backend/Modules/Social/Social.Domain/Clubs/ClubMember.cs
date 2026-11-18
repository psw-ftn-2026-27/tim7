using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class ClubMember : Entity
{
    public Guid UserId { get; private set; }
    public ClubRole Role { get; private set; }

    private ClubMember() { }

    public ClubMember(Guid userId, ClubRole role) : base(Guid.NewGuid())
    {
        UserId = userId;
        Role = role;
    }

    internal void PromoteToModerator()
    {
        if (Role == ClubRole.Moderator)
        {
            throw new DomainException("The member is already a moderator.");
        }

        Role = ClubRole.Moderator;
    }
}
