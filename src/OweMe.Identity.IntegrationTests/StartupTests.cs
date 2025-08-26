using Duende.IdentityModel.Client;
using Duende.IdentityServer.Models;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public sealed class StartupTests: IClassFixture<IntegrationTestSetup>, IClassFixture<UnsecureHttpClientFactory>
{
    private readonly IntegrationTestSetup _setup;
    private readonly UnsecureHttpClientFactory _httpClientFactory;
    
    public StartupTests(IntegrationTestSetup setup, UnsecureHttpClientFactory unsecureHttpClientFactory, ITestOutputHelper testOutputHelper)
    {
        _setup = setup;
        _httpClientFactory = unsecureHttpClientFactory;
        IntegrationTestSetup.InitGlobalLogging(testOutputHelper);
    }
    
    [Fact]
    public async Task Test_DiscoveryDocument_Accessible()
    {
        // Arrange
        var app = await _setup.Create().StartAppAsync();
        
        // Act & Assert
        var urls = app!.Urls;
        Assert.NotEmpty(urls);

        foreach (var url in urls)
        {
            var client = _httpClientFactory.CreateUnsecureClient();
            var disco = await client.GetDiscoveryDocumentAsync(urls.First());
            Assert.False(disco.IsError, $"Discovery document is not accessible at {url}: {disco.Error}");
        }
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
        
        var app = await _setup.Create()
            .Configure<IdentityConfig>(config =>
        {
            config.ApiScopes = [new ApiScope(apiScope)];
            config.Clients =
            [
                new Client
                {
                    ClientId = clientId,
                    ClientSecrets = [new Secret(clientSecret.Sha256())],
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = [apiScope]
                }
            ];
            config.Users =
            [
                new Duende.IdentityServer.Test.TestUser
                {
                    Username = testUserName,
                    Password = testUserPassword,
                    SubjectId = Guid.NewGuid().ToString()
                }
            ];
        })
            .Configure<MigrationsOptions>(options =>
        {
            options.ApplyMigrations = true;
            options.SeedData = true;
        })
            .StartAppAsync();
        
        var urls = app.Urls;
        Assert.NotEmpty(urls);
        
        var client = _httpClientFactory.CreateUnsecureClient();
        var disco = await client.GetDiscoveryDocumentAsync(urls.First());
        Assert.False(disco.IsError, $"Discovery document is not accessible at {urls.First()}: {disco.Error}");
        
        // Act
        var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = clientId,
            ClientSecret = clientSecret,
            UserName = testUserName,
            Password = testUserPassword,
            Scope = apiScope,
        });
        
        // Assert
        Assert.False(tokenResponse.IsError, $"Token request failed: {tokenResponse.Error}");
        Assert.NotNull(tokenResponse.AccessToken);
    }
}