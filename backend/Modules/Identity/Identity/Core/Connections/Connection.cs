using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Identity.Core.Connections;

public sealed class Connection : AggregateRoot
{
    public Guid UserAId { get; private set; }
    public Guid UserBId { get; private set; }
    private readonly List<FollowRequest> _requests = [];
    public IReadOnlyList<FollowRequest> Requests => _requests;

    private Connection() { }

    public Connection(Guid userId, Guid otherUserId) : base(Guid.NewGuid())
    {
        if (userId == otherUserId)
        {
            throw new DomainException("A user cannot be connected to themselves.");
        }

        // The pair is stored in a fixed order, so one pair of users has exactly one row.
        (UserAId, UserBId) = userId.CompareTo(otherUserId) < 0 ? (userId, otherUserId) : (otherUserId, userId);
    }

    public void SendRequest(Guid senderId)
    {
        var recipientId = GetOtherUser(senderId);
        if (FindRequest(senderId, recipientId, FollowRequestStatus.Accepted) is not null)
        {
            throw new DomainException("The user already follows this user.");
        }
        if (FindRequest(senderId, recipientId, FollowRequestStatus.Pending) is not null)
        {
            throw new DomainException("A follow request to this user is already pending.");
        }

        // A new request replaces an earlier rejected one, so one direction holds at most one request.
        var rejectedRequest = FindRequest(senderId, recipientId, FollowRequestStatus.Rejected);
        if (rejectedRequest is not null)
        {
            _requests.Remove(rejectedRequest);
        }

        var request = new FollowRequest(senderId, recipientId);
        _requests.Add(request);

        // A crossed pending request is accepted, and a user who is already followed back
        // is accepted at once.
        FindRequest(recipientId, senderId, FollowRequestStatus.Pending)?.Accept();
        if (FindRequest(recipientId, senderId, FollowRequestStatus.Accepted) is not null)
        {
            request.Accept();
        }
    }

    public void Accept(Guid recipientId)
    {
        GetRequest(GetOtherUser(recipientId), recipientId, FollowRequestStatus.Pending).Accept();
    }

    public void Reject(Guid recipientId)
    {
        GetRequest(GetOtherUser(recipientId), recipientId, FollowRequestStatus.Pending).Reject();
    }

    public void Withdraw(Guid senderId)
    {
        _requests.Remove(GetRequest(senderId, GetOtherUser(senderId), FollowRequestStatus.Pending));
    }

    public void RemoveFollower(Guid userId)
    {
        _requests.Remove(GetRequest(GetOtherUser(userId), userId, FollowRequestStatus.Accepted));
    }

    public void Unfollow(Guid userId)
    {
        _requests.Remove(GetRequest(userId, GetOtherUser(userId), FollowRequestStatus.Accepted));
    }

    private Guid GetOtherUser(Guid userId)
    {
        if (userId == UserAId)
        {
            return UserBId;
        }
        if (userId == UserBId)
        {
            return UserAId;
        }
        throw new DomainException("The user is not part of this connection.");
    }

    private FollowRequest? FindRequest(Guid senderId, Guid recipientId, FollowRequestStatus status)
    {
        return _requests.FirstOrDefault(request => request.SenderId == senderId
            && request.RecipientId == recipientId
            && request.Status == status);
    }

    private FollowRequest GetRequest(Guid senderId, Guid recipientId, FollowRequestStatus status)
    {
        return FindRequest(senderId, recipientId, status)
            ?? throw new DomainException("There is no such follow request between these users.");
    }
}
