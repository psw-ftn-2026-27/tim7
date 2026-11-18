using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogVoting;
using Social.Domain.Blogs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogVoting;

public class BlogVotingCommandTests : BaseIntegrationTest
{
    public BlogVotingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_votes_for_a_published_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new VoteDto(BlogVoteType.Upvote);

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes",
            request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        stored.Votes.Should().ContainSingle(vote =>
            vote.UserId == WellKnownUsers.Administrator && vote.Type == BlogVoteType.Upvote);
    }

    [Fact]
    public async Task User_cannot_vote_for_an_archived_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new VoteDto(BlogVoteType.Upvote);

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{BlogSeed.ArchivedDanubeCruise.Id}/votes",
            request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.ArchivedDanubeCruise.Id);
        stored.Votes.Should().NotContain(vote => vote.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task User_cannot_vote_for_a_blog_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new VoteDto(BlogVoteType.Upvote);

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{Guid.NewGuid()}/votes", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_vote()
    {
        var client = Factory.CreateClient();
        var request = new VoteDto(BlogVoteType.Upvote);

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes",
            request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
