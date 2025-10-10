using Duende.IdentityModel.Client;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using Shouldly;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public sealed class StartupTests : IClassFixture<ProgramFixture>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StartupTests(ITestOutputHelper testOutputHelper, ProgramFixture factory)
    {
        IntegrationTestSetup.InitGlobalLogging(testOutputHelper);
        _factory = factory;
    }

    [Fact]
    public async Task Test_DiscoveryDocument_Accessible()
    {
        // Arrange
        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Configure<MigrationsOptions>(options =>
                    {
                        options.ApplyMigrations = true;
                        options.SeedData = true;
                    });
                });
            })
            .CreateClient();

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

        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Configure<IdentityConfig>(config =>
                    {
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

                    services.Configure<MigrationsOptions>(options =>
                    {
                        options.ApplyMigrations = true;
                        options.SeedData = true;
                    });
                });
            })
            .CreateClient();

        // Act
        client = await client.WithToken(testUserName, testUserPassword, clientId, clientSecret, apiScope);

        // Assert
        client.DefaultRequestHeaders.Authorization.ShouldNotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.ShouldBe("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.ShouldNotBeNullOrEmpty();
    }
}
