using Duende.IdentityModel.Client;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public sealed class StartupTests
{
    public StartupTests(ITestOutputHelper testOutputHelper)
    {
        IntegrationTestSetup.InitGlobalLogging(testOutputHelper);
    }
    
    [Fact]
    public async Task Test_DiscoveryDocument_Accessible()
    {
        // Act
        var app = await IntegrationTestSetup.Create()
            .Configure<MigrationsOptions>(options =>
            {
                options.ApplyMigrations = true;
                options.SeedData = false;
            })
            .WithDatabase()
            .StartAppAsync();
        
        // Assert
        var urls = app!.Urls;
        Assert.NotEmpty(urls);

        foreach (var url in urls)
        {
            var client = UnsecureHttpClientFactory.CreateUnsecureClient();
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

        var app = await IntegrationTestSetup.Create()
            .Configure<IdentityConfig>(config =>
        {
            config.ApiScopes = [new ApiScope(apiScope)];
            config.Clients =
            [
                new Client
                {
                    ClientId = clientId,
                    ClientSecrets = [new Secret(clientSecret)],
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = [apiScope]
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
        })
            .Configure<MigrationsOptions>(options =>
        {
            options.ApplyMigrations = true;
            options.SeedData = true;
        })
            .WithDatabase()
            .StartAppAsync();

        var urls = app.Urls;
        Assert.NotEmpty(urls);
        
        var client = UnsecureHttpClientFactory.CreateUnsecureClient();
        
        // Act
        string token = await PasswordTokenHelper.GetTokenAsync(client, urls.First(), testUserName, testUserPassword,
            clientId, clientSecret, apiScope);
        Assert.NotNull(token);
    }
}