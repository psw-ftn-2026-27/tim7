using System.Globalization;
using System.Net;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringCommandTests : BaseIntegrationTest
{
    public KeyPointAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    // The smallest content the shared upload reader accepts as a 64 x 48 PNG image.
    private static readonly byte[] PngImage =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, .."IHDR"u8, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x30];

    private static MultipartFormDataContent CreateForm(string name, double latitude, double longitude)
    {
        return new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent("Opis tačke."), "Description" },
            { new StringContent(latitude.ToString(CultureInfo.InvariantCulture)), "Location.Latitude" },
            { new StringContent(longitude.ToString(CultureInfo.InvariantCulture)), "Location.Longitude" },
            { new StringContent("Nova tajna"), "Secret" },
            { new StringContent("true"), "IsPublic" }
        };
    }

    private static MultipartFormDataContent CreateFormWithImage(string name, double latitude, double longitude)
    {
        var form = CreateForm(name, latitude, longitude);
        form.Add(new ByteArrayContent(PngImage), "image", "tacka.png");
        return form;
    }

    [Fact]
    public async Task Author_adds_a_key_point_with_an_image_at_the_end_of_their_draft_tour_and_the_length_is_recalculated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishableRiverside;
        using var form = CreateFormWithImage("Štrand", 45.2380, 19.8460);

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id);
        stored.KeyPoints.Should().HaveCount(tour.KeyPoints.Count + 1);
        var added = stored.KeyPoints[^1];
        added.Name.Should().Be("Štrand");
        added.Order.Should().Be(tour.KeyPoints.Count + 1);
        added.Location.Should().Be(new Location(45.2380, 19.8460));
        added.Secret.Should().Be("Nova tajna");
        added.IsPublic.Should().BeTrue();
        added.Image.Should().StartWith("/images/exploration/").And.EndWith(".png");
        stored.Length!.Kilometers.Should().BeGreaterThan(tour.Length!.Kilometers);
    }

    [Fact]
    public async Task Explorer_cannot_add_a_key_point_to_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.PublishableRiverside;
        using var form = CreateFormWithImage("Štrand", 45.2380, 19.8460);

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id);
        stored.KeyPoints.Should().HaveCount(tour.KeyPoints.Count);
    }

    [Fact]
    public async Task Key_point_cannot_be_added_to_a_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyards;
        using var form = CreateFormWithImage("Drugi podrum", 45.1900, 19.9100);

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id);
        stored.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(tour.KeyPoints.Select(keyPoint => keyPoint.Id));
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_key_point()
    {
        var client = Factory.CreateClient();
        using var form = CreateFormWithImage("Štrand", 45.2380, 19.8460);

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = CreateFormWithImage("Štrand", 45.2380, 19.8460);

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_changes_a_key_point_of_their_draft_tour_with_a_new_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishableRiverside;
        var keyPoint = tour.KeyPoints[1];
        using var form = CreateFormWithImage("Štrand", 45.2380, 19.8460);

        var response = await client.PutAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}", form);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id)
            .KeyPoints.Single(storedKeyPoint => storedKeyPoint.Id == keyPoint.Id);
        stored.Name.Should().Be("Štrand");
        stored.Description.Should().Be("Opis tačke.");
        stored.Location.Should().Be(new Location(45.2380, 19.8460));
        stored.Secret.Should().Be("Nova tajna");
        stored.IsPublic.Should().BeTrue();
        stored.Order.Should().Be(keyPoint.Order);
        stored.Image.Should().StartWith("/images/exploration/").And.NotBe(keyPoint.Image);
    }

    [Fact]
    public async Task Changing_a_key_point_without_a_new_image_keeps_its_current_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishableRiverside;
        var keyPoint = tour.KeyPoints[1];
        using var form = CreateForm("Štrand", 45.2380, 19.8460);

        var response = await client.PutAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}", form);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id)
            .KeyPoints.Single(storedKeyPoint => storedKeyPoint.Id == keyPoint.Id);
        stored.Name.Should().Be("Štrand");
        stored.Image.Should().Be(keyPoint.Image);
    }

    [Fact]
    public async Task Explorer_cannot_change_a_key_point_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.PublishableRiverside;
        var keyPoint = tour.KeyPoints[1];
        using var form = CreateForm("Štrand", 45.2380, 19.8460);

        var response = await client.PutAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}", form);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id)
            .KeyPoints.Single(storedKeyPoint => storedKeyPoint.Id == keyPoint.Id);
        stored.Name.Should().Be(keyPoint.Name);
        stored.Location.Should().Be(keyPoint.Location);
    }

    [Fact]
    public async Task Key_point_of_a_published_tour_cannot_be_changed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyards;
        var keyPoint = tour.KeyPoints[1];
        using var form = CreateForm("Drugi podrum", 45.1900, 19.9100);

        var response = await client.PutAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id)
            .KeyPoints.Single(storedKeyPoint => storedKeyPoint.Id == keyPoint.Id);
        stored.Name.Should().Be(keyPoint.Name);
        stored.Location.Should().Be(keyPoint.Location);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_key_point()
    {
        var client = Factory.CreateClient();
        var tour = TourSeed.PublishableRiverside;
        using var form = CreateForm("Štrand", 45.2380, 19.8460);

        var response = await client.PutAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[1].Id}", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_change_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var tour = TourSeed.PublishableRiverside;
        using var form = CreateForm("Štrand", 45.2380, 19.8460);

        var response = await client.PutAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[1].Id}", form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_removes_a_key_point_of_their_draft_tour_and_the_order_and_length_are_recalculated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishableRiverside;
        var removed = tour.KeyPoints[0];
        var remaining = tour.KeyPoints[1];

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{removed.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id);
        stored.KeyPoints.Should().NotContain(keyPoint => keyPoint.Id == removed.Id);
        stored.KeyPoints.Should().ContainSingle(keyPoint => keyPoint.Id == remaining.Id)
            .Which.Order.Should().Be(1);
        stored.Length.Should().BeNull();
    }

    [Fact]
    public async Task Explorer_cannot_remove_a_key_point_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.PublishableRiverside;
        var keyPoint = tour.KeyPoints[0];

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id);
        stored.KeyPoints.Should().Contain(storedKeyPoint => storedKeyPoint.Id == keyPoint.Id);
    }

    [Fact]
    public async Task Key_point_of_a_published_tour_cannot_be_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyards;
        var keyPoint = tour.KeyPoints[0];

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(storedTour => storedTour.Id == tour.Id);
        stored.KeyPoints.Should().Contain(storedKeyPoint => storedKeyPoint.Id == keyPoint.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_key_point()
    {
        var client = Factory.CreateClient();
        var tour = TourSeed.PublishableRiverside;

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[0].Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var tour = TourSeed.PublishableRiverside;

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[0].Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
