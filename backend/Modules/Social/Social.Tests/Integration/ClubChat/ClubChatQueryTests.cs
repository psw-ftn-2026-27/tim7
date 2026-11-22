using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubChat;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubChat;

public class ClubChatQueryTests : BaseIntegrationTest
{
    public ClubChatQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Member_gets_the_messages_with_author_emails()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer1, "explorer");

        var chat = await client.GetFromJsonAsync<ClubChatDto>(
            $"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/chat", JsonOptions);

        chat!.Messages.Items.Should().ContainSingle(message => message.Id == ChatSeed.WalkersDemoMemberGreeting.Id)
            .Which.AuthorEmail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Deleted_message_is_not_in_the_list()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var chat = await client.GetFromJsonAsync<ClubChatDto>(
            $"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/chat", JsonOptions);

        chat!.Messages.Items.Should().NotContain(message => message.Id == ChatSeed.WalkersDeletedMessage.Id);
    }

    [Fact]
    public async Task Member_of_a_club_without_a_chat_gets_an_empty_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var chat = await client.GetFromJsonAsync<ClubChatDto>(
            $"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/chat", JsonOptions);

        chat!.Messages.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Non_member_does_not_get_the_messages()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/chat");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Muted_member_sees_that_they_are_muted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var chat = await client.GetFromJsonAsync<ClubChatDto>(
            $"/api/social/clubs/{ClubSeed.SwimmersWithExplorerMemberAndPendingRequest.Id}/chat", JsonOptions);

        chat!.IsMuted.Should().BeTrue();
    }
}
