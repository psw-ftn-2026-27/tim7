using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Blogs;
using Social.Domain.Blogs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringCommandTests : BaseIntegrationTest
{
    public BlogAuthoringCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_creates_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var request = CreateForm("Novi blog", "Opis novog bloga.", [CreatePng()]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<BlogDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore + 1);
        var stored = assertContext.Blogs.Single(blog => blog.Id == created.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
        stored.Images.Should().ContainSingle().Which.Should().StartWith("/images/social/");
        stored.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Explorer_cannot_create_a_blog_without_a_title()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var request = CreateForm("   ", "Opis novog bloga.", []);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_blog()
    {
        var client = Factory.CreateClient();
        using var request = CreateForm("Novi blog", "Opis novog bloga.", []);

        var response = await client.PostAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var request = CreateForm("Novi blog", "Opis novog bloga.", []);

        var response = await client.PostAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Draft_blog_is_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_blog()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
    }

    [Fact]
    public async Task Author_archives_their_own_published_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        stored.Status.Should().Be(BlogStatus.Archived);
    }

    [Fact]
    public async Task Explorer_cannot_archive_another_authors_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        stored.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public async Task Author_cannot_archive_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_blog()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_archive_a_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_edits_their_own_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var request = EditForm("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/tvrdjava.jpg"], [CreatePng()]);

        var response = await client.PutAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Title.Should().Be("Novi utisci sa tvrđave");
        stored.Description.Should().Be("Dopunjen opis obilaska.");
        stored.Images.Should().HaveCount(2);
        stored.Images[0].Should().Be("https://example.com/tvrdjava.jpg");
        stored.Images[1].Should().StartWith("/images/social/");
        stored.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Explorer_cannot_edit_another_authors_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var request = EditForm("Tuđa izmena", "Izmena tuđeg bloga.", [], []);

        var response = await client.PutAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Title.Should().Be(BlogSeed.FortressImpressions.Title);
        stored.LastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task Author_cannot_save_a_blog_without_a_title()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var request = EditForm("   ", "Dopunjen opis obilaska.", [], []);

        var response = await client.PutAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Title.Should().Be(BlogSeed.FortressImpressions.Title);
        stored.LastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_edit_a_blog()
    {
        var client = Factory.CreateClient();
        using var request = EditForm("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", [], []);

        var response = await client.PutAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_edit_a_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var request = EditForm("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", [], []);

        var response = await client.PutAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // A minimal PNG header of 1 x 1 px; the server reads the format and the dimensions from it.
    private static byte[] CreatePng()
    {
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, .."IHDR"u8, 0, 0, 0, 1, 0, 0, 0, 1];
        return content;
    }

    private static MultipartFormDataContent CreateForm(string title, string description, List<byte[]> images)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(title), "title" },
            { new StringContent(description), "description" }
        };
        foreach (var image in images)
        {
            form.Add(new ByteArrayContent(image), "images", "slika.png");
        }
        return form;
    }

    private static MultipartFormDataContent EditForm(string title, string description, List<string> keptImages, List<byte[]> newImages)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(title), "title" },
            { new StringContent(description), "description" }
        };
        foreach (var address in keptImages)
        {
            form.Add(new StringContent(address), "keptImages");
        }
        foreach (var image in newImages)
        {
            form.Add(new ByteArrayContent(image), "newImages", "nova.png");
        }
        return form;
    }
}
