using System.Net;
using Duende.IdentityModel.Client;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using Shouldly;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Users;

public sealed class GetUserEndpointTests : TestWithLoggingBase, IClassFixture<ProgramFixture>
{
    private const string TestUserName = "alice";
    private const string TestUserPassword = "Password1#";
    private const string ClientId = "client";
    private const string ClientSecret = "secret";
    private const string ApiScope = "api1";
    private const string LocalApiClientId = "local_api_client";
    private const string LocalApiClientSecret = "local_api_secret";

    private static readonly Action<IdentityConfig> ConfigureIdentityConfig =
        config =>
        {
            config.ApiScopes =
            [
                new ApiScope(ApiScope),
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
            ];
            config.Clients =
            [
                new Client
                {
                    ClientId = ClientId,
                    ClientSecrets = [new Secret(ClientSecret)],
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = [ApiScope, "openid", "profile"]
                },
                new Client
                {
                    ClientId = LocalApiClientId,
                    ClientSecrets = [new Secret(LocalApiClientSecret)],
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = [IdentityServerConstants.LocalApi.ScopeName]
                }
            ];
            config.Users =
            [
                new TestUser
                {
                    Username = TestUserName,
                    Password = TestUserPassword
                }
            ];
        };

    private static readonly Action<MigrationsOptions> ConfigureMigrationsOptions = options =>
    {
        options.ApplyMigrations = true;
        options.SeedData = true;
    };

    private readonly WebApplicationFactory<Program> _factory;
    private readonly Guid _nonExistentUserId = Guid.NewGuid();

    public GetUserEndpointTests(ITestOutputHelper testOutputHelper, ProgramFixture factory) : base(testOutputHelper)
    {
        factory.ConfigureTestServices(builder =>
        {
            builder.WithConfigure(ConfigureMigrationsOptions)
                .WithConfigure(ConfigureIdentityConfig);
        });

        _factory = factory;
    }

    [Fact]
    public async Task For_ClientWithoutProperScope_Should_ReturnUnauthorized()
    {
        // Arrange
        var client = await _factory.CreateClient()
            .WithToken(TestUserName, TestUserPassword, ClientId, ClientSecret, ApiScope);
        var existingUserId = await GetUser(_factory, TestUserName, TestUserPassword);

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
            .WithToken(LocalApiClientId, LocalApiClientSecret, IdentityServerConstants.LocalApi.ScopeName);
        var existingUserId = await GetUser(_factory, TestUserName, TestUserPassword);

        // Act
        var response = await client.GetAsync($"/users/{existingUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();

        var obj = JObject.Parse(content);
        obj.ShouldNotBeNull();
        obj["sub"]?.Value<string>()?.ShouldBe(existingUserId.ToString());
        obj["email"]?.Value<string>()?.ShouldBe(TestUserName);
        obj["userName"]?.Value<string>()?.ShouldBe(TestUserName);
        obj.Properties().Count().ShouldBe(3, "Response should only contain sub, email and userName");
    }

    [Fact]
    public async Task For_NonExistingUser_Should_ReturnNotFound()
    {
        // Arrange
        var client = await _factory.CreateClient()
            .WithToken(LocalApiClientId, LocalApiClientSecret, IdentityServerConstants.LocalApi.ScopeName);

        // Act
        var response = await client.GetAsync($"/users/{_nonExistentUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> GetUser(WebApplicationFactory<Program> factory, string username, string password)
    {
        var client = await factory.CreateClient()
            .WithToken(username, password, ClientId, ClientSecret, $"{ApiScope} openid profile");

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
}
