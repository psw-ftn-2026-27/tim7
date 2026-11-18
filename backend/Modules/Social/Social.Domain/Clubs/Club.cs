using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> Images { get; private set; }
    private readonly List<ClubMember> _members = [];
    public IReadOnlyList<ClubMember> Members => _members;
    private readonly List<JoinRequest> _joinRequests = [];
    public IReadOnlyList<JoinRequest> JoinRequests => _joinRequests;

    private Club()
    {
        Name = null!;
        Description = null!;
        Images = null!;
    }

    public Club(Guid ownerId, string name, string description, List<string>? images)
        : base(Guid.NewGuid())
    {
        var validImages = Validate(name, description, images);

        OwnerId = ownerId;
        Name = name;
        Description = description;
        Images = validImages;
    }

    public void Update(string name, string description, List<string>? images)
    {
        var validImages = Validate(name, description, images);

        Name = name;
        Description = description;
        Images = validImages;
    }

    public bool IsMember(Guid userId)
    {
        return OwnerId == userId || _members.Any(member => member.UserId == userId);
    }

    public bool HasPendingRequest(Guid userId)
    {
        return _joinRequests.Any(request => request.UserId == userId);
    }

    public bool CanResolveRequests(Guid userId)
    {
        return OwnerId == userId || IsModerator(userId);
    }

    public void RequestToJoin(Guid userId)
    {
        if (IsMember(userId))
        {
            throw new DomainException("A member of a club cannot request to join it.");
        }
        if (HasPendingRequest(userId))
        {
            throw new DomainException("A user can have only one pending request to join a club.");
        }

        _joinRequests.Add(new JoinRequest(userId));
    }

    public void WithdrawRequest(Guid userId)
    {
        _joinRequests.Remove(GetPendingRequest(userId));
    }

    public void AcceptRequest(Guid resolverId, Guid userId)
    {
        EnsureCanResolveRequests(resolverId);
        var request = GetPendingRequest(userId);

        _joinRequests.Remove(request);
        _members.Add(new ClubMember(userId, ClubRole.Member));
    }

    public void RejectRequest(Guid resolverId, Guid userId)
    {
        EnsureCanResolveRequests(resolverId);

        _joinRequests.Remove(GetPendingRequest(userId));
    }

    public void PromoteToModerator(Guid promoterId, Guid userId)
    {
        EnsureIsOwner(promoterId, "Only the owner can promote a member to moderator.");
        var member = GetMember(userId);

        member.PromoteToModerator();
    }

    public void TransferOwnership(Guid ownerId, Guid newOwnerId)
    {
        EnsureIsOwner(ownerId, "Only the owner can transfer the ownership of a club.");
        var newOwner = GetMember(newOwnerId);

        _members.Remove(newOwner);
        _members.Add(new ClubMember(OwnerId, ClubRole.Member));
        OwnerId = newOwnerId;
    }

    public void RemoveMember(Guid removerId, Guid userId)
    {
        var removerIsModerator = IsModerator(removerId);
        if (OwnerId != removerId && !removerIsModerator)
        {
            throw new DomainException("Only the owner or a moderator can remove a member from a club.");
        }
        var member = GetMember(userId);
        if (removerIsModerator && member.Role != ClubRole.Member)
        {
            throw new DomainException("A moderator can remove only regular members from a club.");
        }

        _members.Remove(member);
    }

    private bool IsModerator(Guid userId)
    {
        return _members.Any(member => member.UserId == userId && member.Role == ClubRole.Moderator);
    }

    private void EnsureIsOwner(Guid userId, string message)
    {
        if (OwnerId != userId)
        {
            throw new DomainException(message);
        }
    }

    private ClubMember GetMember(Guid userId)
    {
        var member = _members.FirstOrDefault(member => member.UserId == userId);
        if (member is null)
        {
            throw new NotFoundException($"User {userId} is not a member of this club.");
        }
        return member;
    }

    private void EnsureCanResolveRequests(Guid resolverId)
    {
        if (!CanResolveRequests(resolverId))
        {
            throw new DomainException("Only the owner or a moderator can resolve requests to join a club.");
        }
    }

    private JoinRequest GetPendingRequest(Guid userId)
    {
        var request = _joinRequests.FirstOrDefault(request => request.UserId == userId);
        if (request is null)
        {
            throw new NotFoundException($"User {userId} has no pending request to join this club.");
        }
        return request;
    }

    private static List<string> Validate(string name, string description, List<string>? images)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (images is null || images.Count == 0)
        {
            throw new DomainException("A club requires at least one image.");
        }
        return images;
    }
}
