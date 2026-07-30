using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using OweMe.Identity.Migrator.Seeding;
using Client = OweMe.Identity.Migrator.Seeding.Client;

namespace OweMe.Identity.IntegrationTests.Helpers;

internal static class TestSeedData
{
    internal static readonly Scope LocalApiScope = new(){
        Name = IdentityServerConstants.LocalApi.ScopeName,
        DisplayName = "Local API",
        Description = "Access to the local API"
    };

    internal static readonly Scope SomeScope = new(){
        Name = "SomeOtherScope",
        DisplayName = "Some Other Scope",
        Description = "Access to some other scope"
    };

    internal static readonly Scope OpenIdScope = new(){
        Name = IdentityServerConstants.StandardScopes.OpenId,
        DisplayName = "OpenID Connect",
        Description = "OpenID Connect scope"
    };

    internal static readonly Scope ProfileScope = new(){
        Name = IdentityServerConstants.StandardScopes.Profile,
        DisplayName = "Profile",
        Description = "Profile scope"
    };

    internal const string LocalApiClientSecret = "local_api_secret";

    internal static readonly Client LocalApiClient = new()
    {
        ClientId = "local_api_client",
        ClientName = "Local API Client",
        ClientSecrets =
        [
            new Client.Secret
            {
                Value = LocalApiClientSecret.Sha512(),
                Expiration = DateTimeOffset.UtcNow.AddYears(1),
                Type = IdentityServerConstants.SecretTypes.SharedSecret,
                Description = "Local API Client Secret",
            }
        ],
        AllowedScopes = [LocalApiScope.Name],
        AllowedGrantTypes = [GrantType.ClientCredentials],

    };

    internal const string SomeApiClientSecret = "some_api_secret";

    internal static readonly Client SomeApiClient = new()
    {
        ClientId = "some_api_client",
        ClientName = "Some API Client",
        ClientSecrets =
        [
            new Client.Secret
            {
                Value = SomeApiClientSecret.Sha512(),
                Expiration = DateTimeOffset.UtcNow.AddYears(1),
                Type = IdentityServerConstants.SecretTypes.SharedSecret,
                Description = "Some API Client Secret",
            },
        ],
        AllowedGrantTypes = [GrantType.ResourceOwnerPassword],
        AllowedScopes = [SomeScope.Name, OpenIdScope.Name, ProfileScope.Name],
    };

    internal static readonly SeedData Data = new()
    {
        Scopes = [LocalApiScope, SomeScope, OpenIdScope, ProfileScope],
        Clients = [LocalApiClient, SomeApiClient]
    };

    internal static readonly Duende.IdentityServer.Test.TestUser TestUser = new()
    {
        SubjectId = Guid.NewGuid().ToString(),
        Username = "alice",
        Password = "Password1#"
    };
}
