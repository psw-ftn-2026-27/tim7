using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Social.Application.DirectMessaging;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.DirectMessaging;

public class DirectMessagingQueryTests : BaseIntegrationTest
{
    public DirectMessagingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_sees_their_conversation_with_the_other_users_email_and_last_message_time()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer2, "explorer");

        var conversations = await client.GetFromJsonAsync<List<ConversationDto>>("/api/social/conversations", JsonOptions);

        conversations.Should().ContainSingle(conversation => conversation.Id == ConversationSeed.DemoExplorersConversation.Id)
            .Which.Should().Match<ConversationDto>(conversation =>
                conversation.OtherUserId == DemoUsers.Explorer1
                && conversation.OtherUserEmail != null
                && conversation.LastMessageAt == ConversationSeed.DemoExplorerGreeting.SentAt);
    }

    [Fact]
    public async Task Participant_reads_the_conversations_messages()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer2, "explorer");

        var chat = await client.GetFromJsonAsync<ConversationChatDto>(
            $"/api/social/conversations/{ConversationSeed.DemoExplorersConversation.Id}/chat", JsonOptions);

        chat!.Messages.Items.Should().Contain(message => message.Id == ConversationSeed.DemoExplorerGreeting.Id);
    }

    [Fact]
    public async Task Outsider_cannot_read_a_conversation()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer3, "explorer");

        var response = await client.GetAsync($"/api/social/conversations/{ConversationSeed.DemoExplorersConversation.Id}/chat");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
