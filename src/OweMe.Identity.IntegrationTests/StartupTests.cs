using Duende.IdentityModel.Client;
using Duende.IdentityServer.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OweMe.Identity.Server.Setup;
using OweMe.Identity.Server.Users.Persistence;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public class StartupTests(IntegrationTestSetup setup, ITestOutputHelper testOutputHelper) : IClassFixture<IntegrationTestSetup>
{
    [Fact]
    public async Task Test_DiscoveryDocument_Accessible()
    {
        // Arrange
        await setup.StartAppAsync(testOutputHelper);
        
        // Act & Assert
        var urls = setup.app!.Urls;
        Assert.NotEmpty(urls);

        foreach (var url in urls)
        {
            var client = new HttpClient();
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

        setup.Configure<IdentityConfig>(config =>
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
        });
        
        setup.Configure<MigrationsOptions>(options =>
        {
            options.ApplyMigrations = true;
            options.SeedData = true;
        });
        
        await setup.StartAppAsync(testOutputHelper);
        
        var urls = setup.app!.Urls;
        Assert.NotEmpty(urls);
        
        var client = new HttpClient();
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