using Duende.IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.Server.Users.Persistence;

namespace OweMe.Identity.IntegrationTests;

public class UnitTest1(IntegrationTestSetup setup) : IClassFixture<IntegrationTestSetup>
{
    [Fact]
    public async Task Test_OpenIdConfig_Accessible()
    {
        // Arrange
        await setup.StartAppAsync();
        
        // Act & Assert
        var urls = setup.app!.Urls;
        Assert.NotEmpty(urls);
        
        var client = new HttpClient();

        var disco = await client.GetDiscoveryDocumentAsync(urls.First());
        Assert.False(disco.IsError, disco.Error);
    }
}