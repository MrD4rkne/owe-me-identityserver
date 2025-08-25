using Duende.IdentityModel.Client;
using Duende.IdentityServer.Models;
using Microsoft.Extensions.DependencyInjection;
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
        
        setup.Builder.Configuration["OweMe:Identity:ApiScopes:0:Name"] = apiScope;
        
        setup.Builder.Configuration["OweMe:Identity:TestUsers:0:Username"] = testUserName;
        setup.Builder.Configuration["OweMe:Identity:TestUsers:0:Password"] = testUserPassword;
        setup.Builder.Configuration["OweMe:Identity:TestUsers:0:SubjectId"] = Guid.NewGuid().ToString();
        setup.Builder.Configuration["OweMe:Identity:Clients:0:ClientId"] = clientId;
        setup.Builder.Configuration["OweMe:Identity:Clients:0:ClientSecrets:0:Value"] = clientSecret.Sha256();
        setup.Builder.Configuration["OweMe:Identity:Clients:0:AllowedGrantTypes:0"] = "password";
        setup.Builder.Configuration["OweMe:Identity:Clients:0:AllowedScopes:0"] = apiScope;
        
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