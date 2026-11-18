using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourChat;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourChat;

public class TourChatCommandTests : BaseIntegrationTest
{
    public TourChatCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_message_on_a_published_tour_opens_its_chat_and_stores_the_message()
    {
        var tour = TourSeed.PublishedMonasteries;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tour.Id}/chat/messages", new SendMessageDto("Koliko traje obilazak?"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var messageId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var chat = assertContext.Chats.Single(chat => chat.Context.Kind == "tour" && chat.Context.ContextId == tour.Id);
        assertContext.Messages.Should().ContainSingle(message =>
            message.Id == messageId && message.ChatId == chat.Id && message.AuthorId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Draft_tour_has_no_chat_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var countBefore = arrangeContext.Messages.IgnoreQueryFilters().Count();

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/chat/messages", new SendMessageDto("Zdravo!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Messages.IgnoreQueryFilters().Count().Should().Be(countBefore);
    }

    [Fact]
    public async Task Nobody_can_write_in_the_chat_of_an_archived_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var countBefore = arrangeContext.Messages.IgnoreQueryFilters().Count();

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.ArchivedLakeside.Id}/chat/messages", new SendMessageDto("Zdravo!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Messages.IgnoreQueryFilters().Count().Should().Be(countBefore);
    }

    [Fact]
    public async Task Author_deleting_a_visitors_message_marks_it_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/chat/messages/{ChatSeed.VineyardsVisitorQuestion.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Messages.IgnoreQueryFilters()
            .Single(message => message.Id == ChatSeed.VineyardsVisitorQuestion.Id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Author_cannot_delete_a_message_of_another_tours_chat()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/exploration/tours/{TourSeed.ArchivedLakeside.Id}/chat/messages/{ChatSeed.VineyardsVisitorQuestion.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Messages.IgnoreQueryFilters()
            .Single(message => message.Id == ChatSeed.VineyardsVisitorQuestion.Id).IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData("POST", "messages")]
    [InlineData("DELETE", "messages/00000000-0000-0000-0000-0000000000aa")]
    [InlineData("POST", "muted/00000000-0000-0000-0000-0000000000aa")]
    [InlineData("DELETE", "muted/00000000-0000-0000-0000-0000000000aa")]
    public async Task Anonymous_user_cannot_change_a_tour_chat(string method, string path)
    {
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), $"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/chat/{path}")
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
    public async Task Administrator_cannot_change_a_tour_chat(string method, string path)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new HttpRequestMessage(new HttpMethod(method), $"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/chat/{path}")
        {
            Content = JsonContent.Create(new SendMessageDto("Zdravo!"))
        };

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
