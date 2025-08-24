using Duende.IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.Server.Users.Persistence;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public class StartupTests(IntegrationTestSetup setup, ITestOutputHelper testOutputHelper) : IClassFixture<IntegrationTestSetup>
{
    [Fact]
    public async Task Test_OpenIdConfig_Accessible()
    {
        // Arrange
        await setup.StartAppAsync(testOutputHelper);
        
        // Act & Assert
        var urls = setup.app!.Urls;
        Assert.NotEmpty(urls);
        
        var client = new HttpClient();

        var disco = await client.GetDiscoveryDocumentAsync(urls.First());
        Assert.False(disco.IsError, disco.Error);
    }
}