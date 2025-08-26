using Duende.IdentityModel.Client;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OweMe.Identity.Server.Setup;
using OweMe.Identity.Server.Users.Persistence;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OweMe.Identity.IntegrationTests.Setup;

public class MigrationSeedingTests(IntegrationTestSetup setup, ITestOutputHelper testOutputHelper) : IClassFixture<IntegrationTestSetup>
{
    [Fact]
    public async Task MigrationAndSeeding_ShouldNotRun_ByDefault()
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
        
        // Act
        await setup.StartAppAsync(testOutputHelper);
        
        // Assert
        try
        {
            var dbContext = setup.app!.Services.CreateScope().ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            _ = await dbContext.Users.FirstOrDefaultAsync();

            Assert.Fail(
                "Expected exception was not thrown. Database should not be created, thus context.Users should not be accessible.");
        }
        catch (PostgresException ex)
        {
            testOutputHelper.WriteLine($"Expected exception caught: {ex.Message}");
        }
        catch (Exception ex) when (ex is not FailException)
        {
            Assert.Fail("Unexpected exception type caught: " + ex.GetType());
        }
    }
}