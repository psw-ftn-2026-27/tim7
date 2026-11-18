using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);

    private static Club CreateClubWithMember(Guid memberId)
    {
        var club = CreateClub();
        club.RequestToJoin(memberId);
        club.AcceptRequest(WellKnownUsers.Explorer, memberId);
        return club;
    }

    private static Club CreateClubWithModeratorAndMember()
    {
        var club = CreateClubWithMember(WellKnownUsers.SecondExplorer);
        club.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        club.RequestToJoin(WellKnownUsers.Administrator);
        club.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.Administrator);
        return club;
    }

    [Theory]
    [InlineData("", "Vikend ture po stazama Fruške gore.")]
    [InlineData("   ", "Vikend ture po stazama Fruške gore.")]
    [InlineData("Planinari Fruške gore", "")]
    [InlineData("Planinari Fruške gore", "   ")]
    public void New_club_requires_a_name_and_a_description(string name, string description)
    {
        var founding = () => new Club(WellKnownUsers.Explorer, name, description, ["https://example.com/fruska-gora.jpg"]);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_club_requires_at_least_one_image()
    {
        var founding = () => new Club(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", []);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void Change_that_leaves_the_club_without_images_is_rejected_and_the_club_stays_unchanged()
    {
        var club = CreateClub();

        var update = () => club.Update("Novo ime", "Novi opis.", []);

        update.Should().Throw<DomainException>();
        club.Name.Should().Be("Planinari Fruške gore");
        club.Description.Should().Be("Vikend ture po stazama Fruške gore.");
        club.Images.Should().Equal("https://example.com/fruska-gora.jpg");
    }

    [Fact]
    public void Change_may_remove_the_last_image_while_adding_a_new_one()
    {
        var club = CreateClub();

        club.Update("Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/nova.jpg"]);

        club.Images.Should().Equal("https://example.com/nova.jpg");
    }

    [Fact]
    public void Owner_cannot_request_to_join_their_own_club()
    {
        var club = CreateClub();

        var requesting = () => club.RequestToJoin(WellKnownUsers.Explorer);

        requesting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Regular_member_cannot_request_to_join_the_club()
    {
        var club = CreateClubWithMember(WellKnownUsers.SecondExplorer);

        var requesting = () => club.RequestToJoin(WellKnownUsers.SecondExplorer);

        requesting.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_with_a_pending_request_cannot_send_another()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        var requesting = () => club.RequestToJoin(WellKnownUsers.SecondExplorer);

        requesting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Accepted_user_becomes_a_regular_member_and_their_request_disappears()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        club.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.SecondExplorer && member.Role == ClubRole.Member);
        club.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void User_who_did_not_request_to_join_cannot_be_accepted()
    {
        var club = CreateClub();

        var accepting = () => club.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        accepting.Should().Throw<NotFoundException>();
        club.IsMember(WellKnownUsers.SecondExplorer).Should().BeFalse();
    }

    [Fact]
    public void Rejected_request_disappears_and_the_user_is_not_a_member()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        club.RejectRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
        club.IsMember(WellKnownUsers.SecondExplorer).Should().BeFalse();
    }

    [Fact]
    public void Withdrawn_request_disappears()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        club.WithdrawRequest(WellKnownUsers.SecondExplorer);

        club.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Regular_member_cannot_accept_a_request()
    {
        var club = CreateClubWithMember(WellKnownUsers.Administrator);
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        var accepting = () => club.AcceptRequest(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);

        accepting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Regular_member_cannot_reject_a_request()
    {
        var club = CreateClubWithMember(WellKnownUsers.Administrator);
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        var rejecting = () => club.RejectRequest(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);

        rejecting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Moderator_accepts_a_request_to_join()
    {
        var club = CreateClubWithModeratorAndMember();
        var userId = Guid.NewGuid();
        club.RequestToJoin(userId);

        club.AcceptRequest(WellKnownUsers.SecondExplorer, userId);

        club.Members.Should().ContainSingle(member => member.UserId == userId && member.Role == ClubRole.Member);
    }

    [Fact]
    public void Owner_promotes_a_regular_member_to_moderator()
    {
        var club = CreateClubWithMember(WellKnownUsers.SecondExplorer);

        club.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.SecondExplorer && member.Role == ClubRole.Moderator);
    }

    [Fact]
    public void Moderator_cannot_promote_a_member()
    {
        var club = CreateClubWithModeratorAndMember();

        var promoting = () => club.PromoteToModerator(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

        promoting.Should().Throw<DomainException>();
        club.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.Administrator && member.Role == ClubRole.Member);
    }

    [Fact]
    public void Transfer_makes_the_new_owner_the_owner_and_the_old_owner_a_regular_member()
    {
        var club = CreateClubWithModeratorAndMember();

        club.TransferOwnership(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.OwnerId.Should().Be(WellKnownUsers.SecondExplorer);
        club.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
        club.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.Explorer && member.Role == ClubRole.Member);
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_transfer_the_club()
    {
        var club = CreateClubWithModeratorAndMember();

        var transferring = () => club.TransferOwnership(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

        transferring.Should().Throw<DomainException>();
        club.OwnerId.Should().Be(WellKnownUsers.Explorer);
    }

    [Fact]
    public void Owner_cannot_transfer_the_club_to_themselves()
    {
        var club = CreateClubWithModeratorAndMember();

        var transferring = () => club.TransferOwnership(WellKnownUsers.Explorer, WellKnownUsers.Explorer);

        transferring.Should().Throw<NotFoundException>();
        club.OwnerId.Should().Be(WellKnownUsers.Explorer);
        club.Members.Should().NotContain(member => member.UserId == WellKnownUsers.Explorer);
    }

    [Fact]
    public void Owner_removes_a_moderator_who_is_then_no_longer_a_member()
    {
        var club = CreateClubWithModeratorAndMember();

        club.RemoveMember(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.IsMember(WellKnownUsers.SecondExplorer).Should().BeFalse();
    }

    [Fact]
    public void Moderator_removes_a_regular_member()
    {
        var club = CreateClubWithModeratorAndMember();

        club.RemoveMember(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

        club.IsMember(WellKnownUsers.Administrator).Should().BeFalse();
    }

    [Fact]
    public void Moderator_cannot_remove_another_moderator()
    {
        var club = CreateClubWithModeratorAndMember();
        club.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.Administrator);

        var removing = () => club.RemoveMember(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

        removing.Should().Throw<DomainException>();
        club.IsMember(WellKnownUsers.Administrator).Should().BeTrue();
    }

    [Fact]
    public void Regular_member_cannot_remove_anyone()
    {
        var club = CreateClubWithModeratorAndMember();
        var otherMemberId = Guid.NewGuid();
        club.RequestToJoin(otherMemberId);
        club.AcceptRequest(WellKnownUsers.Explorer, otherMemberId);

        var removing = () => club.RemoveMember(WellKnownUsers.Administrator, otherMemberId);

        removing.Should().Throw<DomainException>();
        club.IsMember(otherMemberId).Should().BeTrue();
    }

    [Fact]
    public void Nobody_can_remove_the_owner()
    {
        var club = CreateClubWithModeratorAndMember();

        var removing = () => club.RemoveMember(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);

        removing.Should().Throw<NotFoundException>();
        club.OwnerId.Should().Be(WellKnownUsers.Explorer);
    }
}
