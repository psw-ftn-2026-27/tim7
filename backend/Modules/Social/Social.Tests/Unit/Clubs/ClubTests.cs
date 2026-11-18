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
}
