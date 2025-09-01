using System.Net;
using Duende.IdentityModel.Client;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Builder;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using Shouldly;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public class GetUserEndpointTests : IAsyncLifetime
{
    private const string testUserName = "alice";
    private const string testUserPassword = "Password1#";
    private const string clientId = "client";
    private const string clientSecret = "secret";
    private const string apiScope = "api1";
    private const string localApiClientId = "local_api_client";
    private const string localApiClientSecret = "local_api_secret";

    private readonly Guid userId = Guid.NewGuid();
    private readonly Guid nonExistentUserId = Guid.NewGuid();

    private readonly Action<IdentityConfig> _configureIdentityConfig = config =>
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
                AllowedScopes = [apiScope]
            },
            new Client
            {
                ClientId = localApiClientId,
                ClientSecrets = [new Secret(localApiClientSecret)],
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = [IdentityServerConstants.LocalApi.ScopeName]
            }
        ];
        config.Users =
        [
            new TestUser
            {
                Username = testUserName,
                Password = testUserPassword,
                SubjectId = Guid.NewGuid().ToString()
            }
        ];
    };

    private readonly Action<MigrationsOptions> _configureMigrationsOptions = options =>
    {
        options.ApplyMigrations = true;
        options.SeedData = true;
    };

    private readonly WebApplication _app = null!;

    public GetUserEndpointTests(ITestOutputHelper testOutputHelper)
    {
        IntegrationTestSetup.InitGlobalLogging(testOutputHelper);
    }

    [Fact]
    public async Task For_Client_WithoutProperScope_Should_Return_Unauthorized()
    {
        // Arrange
        var app = await IntegrationTestSetup.Create()
            .Configure(_configureIdentityConfig)
            .Configure(_configureMigrationsOptions)
            .WithDatabase()
            .StartAppAsync();

        var urls = app.Urls;
        urls.ShouldNotBeEmpty();

        var client = UnsecureHttpClientFactory.CreateUnsecureClient();
        string token = await TokenHelper.GetTokenAsync(client, urls.First(), testUserName, testUserPassword, clientId,
            clientSecret, apiScope);
        client.SetBearerToken(token);

        // Act
        var response = await client.GetAsync($"{urls.First()}/users/{userId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task For_ClientWithProperScope_ShouldReturnResult()
    {
        // Arrange
        var app = await IntegrationTestSetup.Create()
            .Configure(_configureIdentityConfig)
            .Configure(_configureMigrationsOptions)
            .WithDatabase()
            .StartAppAsync();

        var urls = app.Urls;
        urls.ShouldNotBeEmpty();

        var client = UnsecureHttpClientFactory.CreateUnsecureClient();
        string token = await TokenHelper.GetTokenAsync(client, urls.First(), localApiClientId, localApiClientSecret,
            IdentityServerConstants.LocalApi.ScopeName);
        client.SetBearerToken(token);

        // Act
        var response = await client.GetAsync($"{urls.First()}/users/{userId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
    }

    public Task InitializeAsync()
    {
        return IntegrationTestSetup.Create()
            .Configure(_configureIdentityConfig)
            .Configure(_configureMigrationsOptions)
            .WithDatabase()
            .StartAppAsync();
    }

    public Task DisposeAsync()
    {
        return _app is not null ? _app.DisposeAsync().AsTask() : Task.CompletedTask;
    }
}