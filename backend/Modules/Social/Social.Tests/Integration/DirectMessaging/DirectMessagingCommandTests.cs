using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Social.Application.Chats;
using Social.Application.DirectMessaging;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.DirectMessaging;

public class DirectMessagingCommandTests : BaseIntegrationTest
{
    public DirectMessagingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Starting_a_conversation_that_exists_returns_it_and_opens_no_second_one()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer2, "explorer");

        var response = await client.PostAsJsonAsync("/api/social/conversations", new StartConversationDto(DemoUsers.Explorer1));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<Guid>(JsonOptions)).Should().Be(ConversationSeed.DemoExplorersConversation.Id);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Conversations.Should().ContainSingle();
    }

    [Fact]
    public async Task Conversation_cannot_be_started_with_a_user_one_does_not_follow()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer3, "explorer");

        var response = await client.PostAsJsonAsync("/api/social/conversations", new StartConversationDto(DemoUsers.Explorer1));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Conversations.Should().ContainSingle();
    }

    [Fact]
    public async Task Participant_sending_a_message_stores_it_in_the_conversations_chat()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer2, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/social/conversations/{ConversationSeed.DemoExplorersConversation.Id}/chat/messages", new SendMessageDto("Idem!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var messageId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Messages.Should().ContainSingle(message =>
            message.Id == messageId && message.ChatId == ConversationSeed.DemoExplorersChat.Id && message.AuthorId == DemoUsers.Explorer2);
    }

    [Fact]
    public async Task Outsider_cannot_write_in_a_conversation_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer3, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var countBefore = arrangeContext.Messages.IgnoreQueryFilters().Count();

        var response = await client.PostAsJsonAsync(
            $"/api/social/conversations/{ConversationSeed.DemoExplorersConversation.Id}/chat/messages", new SendMessageDto("Zdravo!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Messages.IgnoreQueryFilters().Count().Should().Be(countBefore);
    }

    [Theory]
    [InlineData("conversations")]
    [InlineData("conversations/00000000-0000-0000-0000-0000000000aa/chat/messages")]
    public async Task Anonymous_user_cannot_send_direct_messages(string path)
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/social/{path}", new SendMessageDto("Zdravo!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("conversations")]
    [InlineData("conversations/00000000-0000-0000-0000-0000000000aa/chat/messages")]
    public async Task Administrator_cannot_send_direct_messages(string path)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/social/{path}", new SendMessageDto("Zdravo!"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
