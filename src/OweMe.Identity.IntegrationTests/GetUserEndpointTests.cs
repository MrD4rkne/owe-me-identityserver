using System.Net;
using Duende.IdentityModel.Client;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json.Linq;
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

    private WebApplication _app = null!;
    private Guid existingUserId = Guid.NewGuid();

    public GetUserEndpointTests(ITestOutputHelper testOutputHelper)
    {
        IntegrationTestSetup.InitGlobalLogging(testOutputHelper);
    }

    [Fact]
    public async Task For_Client_WithoutProperScope_Should_Return_Unauthorized()
    {
        // Arrange
        var urls = _app.Urls;
        urls.ShouldNotBeEmpty();

        var client = UnsecureHttpClientFactory.CreateUnsecureClient();
        string token = await TokenHelper.GetTokenAsync(client, urls.First(), testUserName, testUserPassword, clientId,
            clientSecret, apiScope);
        client.SetBearerToken(token);

        // Act
        var response = await client.GetAsync($"{urls.First()}/users/{existingUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task For_ClientWithProperScope_ShouldReturnResult()
    {
        // Arrange
        var urls = _app.Urls;
        urls.ShouldNotBeEmpty();

        var client = UnsecureHttpClientFactory.CreateUnsecureClient();
        string token = await TokenHelper.GetTokenAsync(client, urls.First(), localApiClientId, localApiClientSecret,
            IdentityServerConstants.LocalApi.ScopeName);
        client.SetBearerToken(token);

        // Act
        var response = await client.GetAsync($"{urls.First()}/users/{existingUserId}");

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
        var urls = _app.Urls;
        urls.ShouldNotBeEmpty();

        var client = UnsecureHttpClientFactory.CreateUnsecureClient();
        string token = await TokenHelper.GetTokenAsync(client, urls.First(), localApiClientId, localApiClientSecret,
            IdentityServerConstants.LocalApi.ScopeName);
        client.SetBearerToken(token);

        // Act
        var response = await client.GetAsync($"{urls.First()}/users/{nonExistentUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> GetUser(string url, string username, string password)
    {
        var client = UnsecureHttpClientFactory.CreateUnsecureClient();
        string token = await TokenHelper.GetTokenAsync(client, url, username, password, clientId, clientSecret,
            $"{apiScope} openid profile");
        client.SetBearerToken(token);

        // Get IS user info
        UserInfoRequest userInfoRequest = new()
        {
            Address = $"{url}/connect/userinfo",
            Token = token
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
        _app = await IntegrationTestSetup.Create()
            .Configure(_configureIdentityConfig)
            .Configure(_configureMigrationsOptions)
            .WithDatabase()
            .StartAppAsync();

        existingUserId = await GetUser(_app.Urls.First(), testUserName, testUserPassword);
    }

    public Task DisposeAsync()
    {
        return _app is not null ? _app.DisposeAsync().AsTask() : Task.CompletedTask;
    }
}