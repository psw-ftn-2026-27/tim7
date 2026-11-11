using FluentAssertions;
using Identity.Core.Connections;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Unit.Connections;

public class ConnectionTests
{
    private static readonly Guid Sender = WellKnownUsers.Explorer;
    private static readonly Guid Recipient = WellKnownUsers.SecondExplorer;

    private static Connection CreateConnectionWithPendingRequest()
    {
        var connection = new Connection(Sender, Recipient);
        connection.SendRequest(Sender);
        return connection;
    }

    private static Connection CreateConnectionWithAcceptedRequest()
    {
        var connection = CreateConnectionWithPendingRequest();
        connection.Accept(Recipient);
        return connection;
    }

    private static Connection CreateConnectionWithRejectedRequest()
    {
        var connection = CreateConnectionWithPendingRequest();
        connection.Reject(Recipient);
        return connection;
    }

    private static Connection CreateConnectionWithMutualFollowing()
    {
        var connection = CreateConnectionWithAcceptedRequest();
        connection.SendRequest(Recipient);
        return connection;
    }

    [Fact]
    public void User_cannot_be_connected_to_themselves()
    {
        var creation = () => new Connection(Sender, Sender);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Request_is_accepted_at_once_when_the_recipient_already_follows_the_sender()
    {
        var connection = new Connection(Recipient, Sender);
        connection.SendRequest(Recipient);
        connection.Accept(Sender);

        connection.SendRequest(Sender);

        connection.Requests.Should().ContainSingle(request =>
            request.SenderId == Sender && request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public void Crossed_requests_are_both_accepted()
    {
        var connection = new Connection(Recipient, Sender);
        connection.SendRequest(Recipient);

        connection.SendRequest(Sender);

        connection.Requests.Should().HaveCount(2)
            .And.OnlyContain(request => request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public void Second_request_in_the_same_direction_is_refused_while_the_first_is_pending()
    {
        var connection = CreateConnectionWithPendingRequest();

        var sending = () => connection.SendRequest(Sender);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Request_to_an_already_followed_user_is_refused()
    {
        var connection = CreateConnectionWithAcceptedRequest();

        var sending = () => connection.SendRequest(Sender);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_request_after_a_rejection_replaces_the_rejected_one()
    {
        var connection = CreateConnectionWithRejectedRequest();

        connection.SendRequest(Sender);

        connection.Requests.Should().ContainSingle()
            .Which.Should().Match<FollowRequest>(request => request.SenderId == Sender
                && request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public void Accepted_request_makes_the_sender_follow_the_recipient_but_not_the_other_way_round()
    {
        var connection = CreateConnectionWithPendingRequest();

        connection.Accept(Recipient);

        connection.Requests.Should().ContainSingle(request =>
            request.SenderId == Sender && request.RecipientId == Recipient
            && request.Status == FollowRequestStatus.Accepted);
        connection.Requests.Should().NotContain(request => request.SenderId == Recipient);
    }

    [Fact]
    public void Request_that_is_not_pending_cannot_be_accepted()
    {
        var connection = CreateConnectionWithRejectedRequest();

        var accepting = () => connection.Accept(Recipient);

        accepting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Request_that_is_not_pending_cannot_be_rejected()
    {
        var connection = CreateConnectionWithAcceptedRequest();

        var rejecting = () => connection.Reject(Recipient);

        rejecting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Sender_withdraws_a_pending_request()
    {
        var connection = CreateConnectionWithPendingRequest();

        connection.Withdraw(Sender);

        connection.Requests.Should().BeEmpty();
    }

    [Fact]
    public void Sender_cannot_withdraw_a_request_that_is_not_pending()
    {
        var connection = CreateConnectionWithRejectedRequest();

        var withdrawal = () => connection.Withdraw(Sender);

        withdrawal.Should().Throw<DomainException>();
    }

    [Fact]
    public void Removing_a_follower_ends_only_their_following_and_the_opposite_following_stays()
    {
        var connection = CreateConnectionWithMutualFollowing();

        connection.RemoveFollower(Recipient);

        connection.Requests.Should().ContainSingle()
            .Which.Should().Match<FollowRequest>(request => request.SenderId == Recipient
                && request.RecipientId == Sender
                && request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public void Unfollowing_ends_only_that_direction_and_the_opposite_following_stays()
    {
        var connection = CreateConnectionWithMutualFollowing();

        connection.Unfollow(Sender);

        connection.Requests.Should().ContainSingle()
            .Which.Should().Match<FollowRequest>(request => request.SenderId == Recipient
                && request.RecipientId == Sender
                && request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public void User_who_does_not_follow_the_user_cannot_be_removed_as_a_follower()
    {
        var connection = CreateConnectionWithAcceptedRequest();

        var removal = () => connection.RemoveFollower(Sender);

        removal.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_unfollow_a_user_they_do_not_follow()
    {
        var connection = CreateConnectionWithAcceptedRequest();

        var unfollowing = () => connection.Unfollow(Recipient);

        unfollowing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Removed_follower_may_send_a_new_request()
    {
        var connection = CreateConnectionWithAcceptedRequest();
        connection.RemoveFollower(Recipient);

        connection.SendRequest(Sender);

        connection.Requests.Should().ContainSingle(request =>
            request.SenderId == Sender && request.Status == FollowRequestStatus.Pending);
    }
}
