using Shared.Domain;

namespace Identity.Core.Connections;

public sealed class FollowRequest : Entity
{
    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public FollowRequestStatus Status { get; private set; }

    private FollowRequest() { }

    public FollowRequest(Guid senderId, Guid recipientId) : base(Guid.NewGuid())
    {
        SenderId = senderId;
        RecipientId = recipientId;
        Status = FollowRequestStatus.Pending;
    }

    public void Accept()
    {
        Status = FollowRequestStatus.Accepted;
    }

    public void Reject()
    {
        Status = FollowRequestStatus.Rejected;
    }
}
