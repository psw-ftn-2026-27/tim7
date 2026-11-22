using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Social.Application.Chats;
using Social.Application.ClubChat;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubChat;

public class ClubChatCommandTests : BaseIntegrationTest
{
    public ClubChatCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_message_in_a_club_opens_its_chat_and_stores_the_message()
    {
        var club = ClubSeed.FruskaGoraHikers;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/social/clubs/{club.Id}/chat/messages", new SendMessageDto("Ko ide u subotu?"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var messageId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var chat = assertContext.Chats.Single(chat => chat.Context.Kind == "club" && chat.Context.ContextId == club.Id);
        assertContext.Messages.Should().ContainSingle(message =>
            message.Id == messageId && message.ChatId == chat.Id
            && message.AuthorId == WellKnownUsers.Explorer && message.Text == "Ko ide u subotu?");
    }

    [Fact]
    public async Task Message_in_a_club_that_has_a_chat_does_not_open_a_second_chat()
    {
        var club = ClubSeed.WalkersWithDemoMember;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/social/clubs/{club.Id}/chat/messages", new SendMessageDto("Važi."));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Chats.Where(chat => chat.Context.ContextId == club.Id).Select(chat => chat.Id)
            .Should().Equal(ChatSeed.WalkersChat.Id);
    }

    [Fact]
    public async Task Non_member_cannot_send_a_message_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var countBefore = arrangeContext.Messages.IgnoreQueryFilters().Count();

        var response = await client.PostAsJsonAsync(
            $"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/chat/messages", new SendMessageDto("Zdravo!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Messages.IgnoreQueryFilters().Count().Should().Be(countBefore);
    }

    [Fact]
    public async Task Muted_member_cannot_send_a_message_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var countBefore = arrangeContext.Messages.IgnoreQueryFilters().Count();

        var response = await client.PostAsJsonAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersWithExplorerMemberAndPendingRequest.Id}/chat/messages", new SendMessageDto("Zdravo!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Messages.IgnoreQueryFilters().Count().Should().Be(countBefore);
    }

    [Fact]
    public async Task Owner_deleting_a_members_message_marks_it_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/chat/messages/{ChatSeed.WalkersDemoMemberGreeting.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Messages.IgnoreQueryFilters()
            .Single(message => message.Id == ChatSeed.WalkersDemoMemberGreeting.Id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Moderator_cannot_delete_a_message_of_another_clubs_chat()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/chat/messages/{ChatSeed.WalkersDemoMemberGreeting.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Messages.IgnoreQueryFilters()
            .Single(message => message.Id == ChatSeed.WalkersDemoMemberGreeting.Id).IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Owner_muting_a_member_stores_the_mute()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/chat/muted/{DemoUsers.Explorer1}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Chats.Single(chat => chat.Id == ChatSeed.WalkersChat.Id).MutedUsers
            .Should().ContainSingle(muted => muted.UserId == DemoUsers.Explorer1);
    }

    [Theory]
    [InlineData("POST", "messages")]
    [InlineData("DELETE", "messages/00000000-0000-0000-0000-0000000000aa")]
    [InlineData("POST", "muted/00000000-0000-0000-0000-0000000000aa")]
    [InlineData("DELETE", "muted/00000000-0000-0000-0000-0000000000aa")]
    public async Task Anonymous_user_cannot_change_a_club_chat(string method, string path)
    {
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), $"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/chat/{path}")
        {
            Content = JsonContent.Create(new SendMessageDto("Zdravo!"))
        };

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("POST", "messages")]
    [InlineData("DELETE", "messages/00000000-0000-0000-0000-0000000000aa")]
    [InlineData("POST", "muted/00000000-0000-0000-0000-0000000000aa")]
    [InlineData("DELETE", "muted/00000000-0000-0000-0000-0000000000aa")]
    public async Task Administrator_cannot_change_a_club_chat(string method, string path)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new HttpRequestMessage(new HttpMethod(method), $"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/chat/{path}")
        {
            Content = JsonContent.Create(new SendMessageDto("Zdravo!"))
        };

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
