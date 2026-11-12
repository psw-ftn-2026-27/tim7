using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.Clubs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubBrowsing;

public class ClubBrowsingQueryTests : BaseIntegrationTest
{
    public ClubBrowsingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Page_continues_the_list_of_all_clubs_by_name_and_then_by_identifier()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expectedIds = OrderedSeedIds().Skip(3).Take(3);

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs?page=2&pageSize=3", JsonOptions);

        clubs!.Items.Select(club => club.Id).Should().Equal(expectedIds);
    }

    [Fact]
    public async Task Total_count_equals_the_number_of_all_clubs()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs?pageSize=1", JsonOptions);

        clubs!.TotalCount.Should().Be(ClubSeed.All.Length);
    }

    [Fact]
    public async Task Page_and_page_size_below_the_bounds_are_brought_within_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expectedIds = OrderedSeedIds().Take(1);

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs?page=0&pageSize=0", JsonOptions);

        clubs!.Items.Select(club => club.Id).Should().Equal(expectedIds);
    }

    [Fact]
    public async Task Anonymous_user_cannot_browse_clubs()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/clubs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static IEnumerable<Guid> OrderedSeedIds()
    {
        return ClubSeed.All
            .OrderBy(club => club.Name, StringComparer.Ordinal)
            .ThenBy(club => club.Id)
            .Select(club => club.Id);
    }
}
