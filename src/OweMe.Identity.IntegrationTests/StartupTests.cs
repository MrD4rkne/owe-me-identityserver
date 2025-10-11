using Duende.IdentityModel.Client;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using Shouldly;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public sealed class StartupTests : TestWithLoggingBase, IClassFixture<ProgramFixture>
{
    private readonly ProgramFixture _factory;

    public StartupTests(ITestOutputHelper testOutputHelper, ProgramFixture factory)
        : base(testOutputHelper)
    {
        _factory = factory;

        // Configure the server with migrations options
        _factory.ConfigureTestServices(builder =>
        {
            builder.WithConfigure<MigrationsOptions>(options =>
            {
                options.ApplyMigrations = true;
                options.SeedData = true;
            });

            // Configure identity options for all tests
            builder.WithConfigure<IdentityConfig>(config =>
            {
                const string apiScope = "api1";
                const string clientId = "client";
                const string clientSecret = "secret";
                const string testUserName = "alice";
                const string testUserPassword = "Password1#";

                config.ApiScopes =
                [
                    new ApiScope(apiScope),
                    new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
                ];

                config.Clients =
                [
                    new Client
                    {
                        ClientId = clientId,
                        ClientSecrets = [new Secret(clientSecret)],
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                        AllowedScopes = [apiScope, "openid", "profile"]
                    }
                ];

                config.Users =
                [
                    new TestUser
                    {
                        Username = testUserName,
                        Password = testUserPassword
                    }
                ];
            });
        });
    }

    [Fact]
    public async Task Test_DiscoveryDocument_Accessible()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var disco = await client.GetDiscoveryDocumentAsync();

        // Assert
        Assert.False(disco.IsError, $"Discovery document is not accessible: {disco.Error}");
        Assert.NotNull(disco.TokenEndpoint);
    }

    [Fact]
    public async Task After_Seeding_TestUser_Can_Request_Token()
    {
        // Arrange
        const string testUserName = "alice";
        const string testUserPassword = "Password1#";
        const string clientId = "client";
        const string clientSecret = "secret";
        const string apiScope = "api1";

        var client = _factory.CreateClient();

        // Act
        client = await client.WithToken(testUserName, testUserPassword, clientId, clientSecret, apiScope);

        // Assert
        client.DefaultRequestHeaders.Authorization.ShouldNotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.ShouldBe("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.ShouldNotBeNullOrEmpty();
    }
}
