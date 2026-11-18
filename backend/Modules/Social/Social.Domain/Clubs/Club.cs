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

    public void RequestToJoin(Guid userId)
    {
        if (IsMember(userId))
        {
            throw new DomainException("A member of a club cannot request to join it.");
        }
        if (_joinRequests.Any(request => request.UserId == userId))
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

    private void EnsureCanResolveRequests(Guid resolverId)
    {
        var isModerator = _members.Any(member => member.UserId == resolverId && member.Role == ClubRole.Moderator);
        if (OwnerId != resolverId && !isModerator)
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
