using System.Net;
using Duende.IdentityModel.Client;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using Shouldly;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public sealed class GetUserEndpointTests : IAsyncLifetime, IClassFixture<WebApplicationFactory<Program>>
{
    private const string testUserName = "alice";
    private const string testUserPassword = "Password1#";
    private const string clientId = "client";
    private const string clientSecret = "secret";
    private const string apiScope = "api1";
    private const string localApiClientId = "local_api_client";
    private const string localApiClientSecret = "local_api_secret";

    private readonly Guid nonExistentUserId = Guid.NewGuid();

    private static readonly Action<IdentityConfig> _configureIdentityConfig =
        config =>
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
                Password = testUserPassword
            }
        ];
    };

    private readonly Action<MigrationsOptions> _configureMigrationsOptions = options =>
    {
        options.ApplyMigrations = true;
        options.SeedData = true;
    };

    private Guid existingUserId = Guid.NewGuid();

    private readonly WebApplicationFactory<Program> _factory;

    public GetUserEndpointTests(ITestOutputHelper testOutputHelper, WebApplicationFactory<Program> factory)
    {
        IntegrationTestSetup.InitGlobalLogging(testOutputHelper);
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure(_configureIdentityConfig);
                services.Configure(_configureMigrationsOptions);
            });
        });
    }

    [Fact]
    public async Task For_ClientWithoutProperScope_Should_ReturnUnauthorized()
    {
        // Arrange
        var client = await _factory.CreateClient()
            .WithToken(testUserName, testUserPassword, clientId, clientSecret, apiScope);

        // Act
        var response = await client.GetAsync($"/users/{existingUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task For_ClientWithProperScope_ShouldReturnResult()
    {
        // Arrange
        var client = await _factory.CreateClient()
            .WithToken(localApiClientId, localApiClientSecret, IdentityServerConstants.LocalApi.ScopeName);

        // Act
        var response = await client.GetAsync($"/users/{existingUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();

        var obj = JObject.Parse(content);
        obj.ShouldNotBeNull();
        obj["sub"]?.Value<string>()?.ShouldBe(existingUserId.ToString());
        obj["email"]?.Value<string>()?.ShouldBe(testUserName);
        obj["userName"]?.Value<string>()?.ShouldBe(testUserName);
        obj.Properties().Count().ShouldBe(3, "Response should only contain sub, email and userName");
    }

    [Fact]
    public async Task For_NonExistingUser_Should_ReturnNotFound()
    {
        // Arrange
        var client = await _factory.CreateClient()
            .WithToken(localApiClientId, localApiClientSecret, IdentityServerConstants.LocalApi.ScopeName);

        // Act
        var response = await client.GetAsync($"/users/{nonExistentUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> GetUser(WebApplicationFactory<Program> factory, string username, string password)
    {
        var client = await factory.CreateClient()
            .WithToken(localApiClientId, localApiClientSecret, $"{apiScope} openid profile");

        // Get IS user info
        UserInfoRequest userInfoRequest = new()
        {
            Address = "/connect/userinfo",
            Token = client.DefaultRequestHeaders.Authorization?.Parameter
        };

        var userInfoResponse = await client.GetUserInfoAsync(userInfoRequest);
        userInfoResponse.IsError.ShouldBeFalse();
        userInfoResponse.Claims.ShouldNotBeEmpty();

        var subClaim = userInfoResponse.Claims.FirstOrDefault(c => c.Type == "sub");
        subClaim.ShouldNotBeNull();
        return Guid.Parse(subClaim.Value);
    }

    public async Task InitializeAsync()
    {
        existingUserId = await GetUser(_factory, testUserName, testUserPassword);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
