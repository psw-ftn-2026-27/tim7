using System.Text.Json;
using System.Text.Json.Serialization;
using Identity.Infrastructure;
using Identity.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<IdentityApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly IdentityApiFactory Factory;

    protected BaseIntegrationTest(IdentityApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<IdentityModuleDbContext>(IdentitySeed.All);
    }
}
