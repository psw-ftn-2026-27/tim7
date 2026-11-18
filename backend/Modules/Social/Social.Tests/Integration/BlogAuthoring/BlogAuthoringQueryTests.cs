using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Blogs;
using Social.Domain.Blogs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringQueryTests : BaseIntegrationTest
{
    public BlogAuthoringQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_lists_only_their_own_blogs()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var blogs = await client.GetFromJsonAsync<List<BlogDto>>("/api/social/blogs/mine", JsonOptions);

        blogs.Should().HaveCount(BlogSeed.All.Count(blog => blog.AuthorId == WellKnownUsers.Explorer));
        blogs.Should().OnlyContain(blog => blog.AuthorId == WellKnownUsers.Explorer);
        blogs.Should().NotContain(blog => blog.Id == BlogSeed.SecondExplorersDiary.Id);
    }

    [Fact]
    public async Task Author_loads_their_own_draft_for_editing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var blog = await client.GetFromJsonAsync<BlogDto>($"/api/social/blogs/mine/{BlogSeed.FortressImpressions.Id}", JsonOptions);

        blog!.Id.Should().Be(BlogSeed.FortressImpressions.Id);
        blog.Status.Should().Be(BlogStatus.Draft);
        blog.Title.Should().Be(BlogSeed.FortressImpressions.Title);
    }

    [Fact]
    public async Task Author_cannot_load_another_authors_blog_for_editing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/blogs/mine/{BlogSeed.SecondExplorersDiary.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
