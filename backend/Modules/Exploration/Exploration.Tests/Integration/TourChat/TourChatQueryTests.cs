using System.Net.Http.Json;
using Exploration.Application.TourChat;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourChat;

public class TourChatQueryTests : BaseIntegrationTest
{
    public TourChatQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_is_privileged_in_the_tours_chat_and_sees_the_messages()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var chat = await client.GetFromJsonAsync<TourChatDto>(
            $"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/chat", JsonOptions);

        chat!.IsPrivileged.Should().BeTrue();
        chat.Messages.Items.Should().Contain(message => message.Id == ChatSeed.VineyardsVisitorQuestion.Id);
    }

    [Fact]
    public async Task Visitor_is_not_privileged_in_the_tours_chat()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var chat = await client.GetFromJsonAsync<TourChatDto>(
            $"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/chat", JsonOptions);

        chat!.IsPrivileged.Should().BeFalse();
    }
}
