using Xunit.Abstractions;

namespace OweMe.Identity.Migrator.IntegrationTests;

public sealed class SeedCommand(AppFixture fixture, ITestOutputHelper testOutputHelper) : IClassFixture<AppFixture>
{
    [Fact]
    public async Task EmptySeed_Should_CompleteSuccessfully()
    {
        // Arrange
        var sut = fixture.Build(testOutputHelper);

        // Act
        var result = await sut.Parse("seed").InvokeAsync();

        // Assert
        Assert.Equal(0, result);
    }
}
