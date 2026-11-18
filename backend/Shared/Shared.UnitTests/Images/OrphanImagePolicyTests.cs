using FluentAssertions;
using Shared.Domain.Images;
using Xunit;

namespace Shared.UnitTests.Images;

public class OrphanImagePolicyTests
{
    private static readonly DateTime Now = new(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan MinAge = TimeSpan.FromDays(1);

    private static StoredImage CreateImage(string module, string name, TimeSpan age) =>
        new(module, $"/images/{module}/{name}", Now - age);

    private static ImageUsageReport CreateReport(string module, params string[] addresses) =>
        new(module, addresses.ToHashSet());

    [Fact]
    public void Orphan_of_a_reporting_module_older_than_the_minimum_age_is_selected()
    {
        var orphan = CreateImage("social", "orphan.png", TimeSpan.FromDays(2));

        var selected = OrphanImagePolicy.SelectForDeletion([orphan], [CreateReport("social")], Now, MinAge);

        selected.Should().Equal(orphan);
    }

    [Fact]
    public void Image_in_use_is_never_selected()
    {
        var used = CreateImage("social", "used.png", TimeSpan.FromDays(30));

        var selected = OrphanImagePolicy.SelectForDeletion([used], [CreateReport("social", used.Address)], Now, MinAge);

        selected.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(TimeSpan.TicksPerHour)]
    [InlineData(TimeSpan.TicksPerDay)]
    public void Orphan_younger_than_the_minimum_age_stays(long ticksUnderMinimumAge)
    {
        var orphan = CreateImage("social", "orphan.png", MinAge - TimeSpan.FromTicks(ticksUnderMinimumAge));

        var selected = OrphanImagePolicy.SelectForDeletion([orphan], [CreateReport("social")], Now, MinAge);

        selected.Should().BeEmpty();
    }

    [Fact]
    public void Nothing_is_selected_when_any_module_did_not_report_its_images()
    {
        var orphan = CreateImage("social", "orphan.png", TimeSpan.FromDays(2));
        ImageUsageReport[] reports = [CreateReport("social"), new ImageUsageReport("exploration", null)];

        var selected = OrphanImagePolicy.SelectForDeletion([orphan], reports, Now, MinAge);

        selected.Should().BeEmpty();
    }

    [Fact]
    public void Image_of_a_module_that_does_not_report_its_images_stays()
    {
        var unreported = CreateImage("exploration", "tour.png", TimeSpan.FromDays(2));

        var selected = OrphanImagePolicy.SelectForDeletion([unreported], [CreateReport("social")], Now, MinAge);

        selected.Should().BeEmpty();
    }
}
