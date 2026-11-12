using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubManagement;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubManagement;

public class ClubManagementCommandTests : BaseIntegrationTest
{
    public ClubManagementCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_founds_a_club_and_becomes_its_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajednički obilasci muzeja i galerija.", ["https://example.com/muzej.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore + 1);
        var stored = assertContext.Clubs.Single(club => club.Id == createdId);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.Name.Should().Be("Ljubitelji muzeja");
        stored.Description.Should().Be("Zajednički obilasci muzeja i galerija.");
        stored.Images.Should().Equal("https://example.com/muzej.jpg");
    }

    [Fact]
    public async Task Club_that_breaks_a_club_rule_is_not_founded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajednički obilasci muzeja i galerija.", []);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_found_a_club()
    {
        var client = Factory.CreateClient();
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajednički obilasci muzeja i galerija.", ["https://example.com/muzej.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_found_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajednički obilasci muzeja i galerija.", ["https://example.com/muzej.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_updates_their_own_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Planinari i šetači", "Ture i šetnje po Fruškoj gori.", ["https://example.com/vidikovac.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.FruskaGoraHikers.Id);
        stored.Name.Should().Be("Planinari i šetači");
        stored.Description.Should().Be("Ture i šetnje po Fruškoj gori.");
        stored.Images.Should().Equal("https://example.com/vidikovac.jpg");
    }

    [Fact]
    public async Task Explorer_cannot_update_another_owners_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new UpdateClubDto("Tuđa izmena", "Izmena tuđeg kluba.", ["https://example.com/tudja.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.FruskaGoraHikers.Id);
        stored.Name.Should().Be(ClubSeed.FruskaGoraHikers.Name);
        stored.Description.Should().Be(ClubSeed.FruskaGoraHikers.Description);
        stored.Images.Should().Equal(ClubSeed.FruskaGoraHikers.Images);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_club()
    {
        var client = Factory.CreateClient();
        var request = new UpdateClubDto("Planinari i šetači", "Ture i šetnje po Fruškoj gori.", ["https://example.com/vidikovac.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateClubDto("Planinari i šetači", "Ture i šetnje po Fruškoj gori.", ["https://example.com/vidikovac.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_deletes_their_own_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Should().NotContain(club => club.Id == ClubSeed.FruskaGoraHikers.Id);
    }

    [Fact]
    public async Task Explorer_cannot_delete_another_owners_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Should().Contain(club => club.Id == ClubSeed.FruskaGoraHikers.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
