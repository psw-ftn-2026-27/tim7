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
        UserAId = userId.CompareTo(otherUserId) < 0 ? userId : otherUserId;
        UserBId = userId.CompareTo(otherUserId) < 0 ? otherUserId : userId;
    }

    public void SendRequest(Guid senderId)
    {
        var recipientId = GetOtherUser(senderId);
        if (Follows(senderId, recipientId))
        {
            throw new DomainException("The user already follows this user.");
        }
        if (FindPendingRequest(senderId, recipientId) is not null)
        {
            throw new DomainException("A follow request to this user is already pending.");
        }

        var request = new FollowRequest(senderId, recipientId);
        _requests.Add(request);

        var crossedRequest = FindPendingRequest(recipientId, senderId);
        if (crossedRequest is not null)
        {
            crossedRequest.Accept();
            request.Accept();
        }
        else if (Follows(recipientId, senderId))
        {
            request.Accept();
        }
    }

    public void Accept(Guid recipientId)
    {
        GetPendingRequest(GetOtherUser(recipientId), recipientId).Accept();
    }

    public void Reject(Guid recipientId)
    {
        GetPendingRequest(GetOtherUser(recipientId), recipientId).Reject();
    }

    public void Withdraw(Guid senderId)
    {
        _requests.Remove(GetPendingRequest(senderId, GetOtherUser(senderId)));
    }

    public void RemoveFollower(Guid userId)
    {
        _requests.Remove(GetAcceptedRequest(GetOtherUser(userId), userId));
    }

    public void Unfollow(Guid userId)
    {
        _requests.Remove(GetAcceptedRequest(userId, GetOtherUser(userId)));
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

    private bool Follows(Guid followerId, Guid followedId)
    {
        return _requests.Any(request => request.SenderId == followerId
            && request.RecipientId == followedId
            && request.Status == FollowRequestStatus.Accepted);
    }

    private FollowRequest GetAcceptedRequest(Guid followerId, Guid followedId)
    {
        var request = _requests.FirstOrDefault(request => request.SenderId == followerId
            && request.RecipientId == followedId
            && request.Status == FollowRequestStatus.Accepted);
        if (request is null)
        {
            throw new DomainException("The user does not follow this user.");
        }
        return request;
    }

    private FollowRequest? FindPendingRequest(Guid senderId, Guid recipientId)
    {
        return _requests.FirstOrDefault(request => request.SenderId == senderId
            && request.RecipientId == recipientId
            && request.Status == FollowRequestStatus.Pending);
    }

    private FollowRequest GetPendingRequest(Guid senderId, Guid recipientId)
    {
        var request = FindPendingRequest(senderId, recipientId);
        if (request is null)
        {
            throw new DomainException("There is no pending follow request between these users.");
        }
        return request;
    }
}
