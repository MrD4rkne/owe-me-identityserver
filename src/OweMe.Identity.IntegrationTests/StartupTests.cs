using Duende.IdentityModel.Client;
using OweMe.Identity.IntegrationTests.Helpers;
using Shouldly;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public sealed class StartupTests(ITestOutputHelper testOutputHelper, ProgramWithSeedData factory)
    : IClassFixture<ProgramWithSeedData>
{
    private readonly ProgramFixture _factory = factory.WithTestOutputHelper(testOutputHelper);

    [Fact]
    public async Task Test_DiscoveryDocument_Accessible()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var disco = await client.GetDiscoveryDocumentAsync();

        // Assert
        Assert.False(disco.IsError, $"Discovery document is not accessible: {disco.Error}");
        Assert.NotNull(disco.TokenEndpoint);
    }

    [Fact]
    public async Task After_Seeding_TestUser_Can_Request_Token()
    {
        // Arrange
        var httpClient = _factory.CreateClient();

        var user = TestSeedData.TestUser;
        var scope = TestSeedData.SomeScope;
        var client = TestSeedData.SomeApiClient;
        var clientSecret = TestSeedData.SomeApiClientSecret;

        // Act
        var response = await httpClient.WithToken(
            user.Username,
            user.Password,
            client.ClientId,
            clientSecret,
            scope.Name);

        // Assert
        response.DefaultRequestHeaders.Authorization.ShouldNotBeNull();
        response.DefaultRequestHeaders.Authorization!.Scheme.ShouldBe("Bearer");
        response.DefaultRequestHeaders.Authorization.Parameter.ShouldNotBeNullOrEmpty();
    }
}
