using FluentAssertions;
using Identity.Contracts;
using Identity.Tests.Integration.Seeds;
using Microsoft.Extensions.DependencyInjection;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Integration.Contracts;

public class IdentityApiTests : BaseIntegrationTest
{
    public IdentityApiTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Users_are_found_by_their_ids_and_unknown_ids_are_left_out()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();
        var unknownId = Guid.Parse("00000000-0000-0000-0009-000000000001");

        var users = await identityApi.GetUsersByIdsAsync([WellKnownUsers.Explorer, UserSeed.Administrator.Id, unknownId]);

        users.Should().BeEquivalentTo([
            new UserSummary(WellKnownUsers.Explorer, UserSeed.Explorer.Email!),
            new UserSummary(UserSeed.Administrator.Id, UserSeed.Administrator.Email!)
        ]);
    }

    [Fact]
    public async Task Search_finds_explorers_by_part_of_their_username_regardless_of_case()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var found = await identityApi.SearchExplorersAsync("Explorer");

        found.Should().Contain(new UserSummary(UserSeed.LoneExplorer.Id, UserSeed.LoneExplorer.Email!));
        found.Should().Contain(new UserSummary(WellKnownUsers.SecondExplorer, UserSeed.SecondExplorer.Email!));
    }

    [Fact]
    public async Task Search_leaves_out_administrators_even_when_their_username_matches()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var found = await identityApi.SearchExplorersAsync("admin");

        found.Should().NotContain(user => user.Id == UserSeed.Administrator.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Search_with_a_blank_username_finds_nobody(string username)
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var found = await identityApi.SearchExplorersAsync(username);

        found.Should().BeEmpty();
    }
}
