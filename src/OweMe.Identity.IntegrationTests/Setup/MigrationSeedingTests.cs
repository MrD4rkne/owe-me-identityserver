using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OweMe.Identity.Server.Setup;
using OweMe.Identity.Server.Users.Persistence;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OweMe.Identity.IntegrationTests.Setup;

public sealed class MigrationSeedingTests: IClassFixture<IntegrationTestSetup>
{
    private readonly IntegrationTestSetup _setup;
    private ITestOutputHelper _testOutputHelper;

    public MigrationSeedingTests(IntegrationTestSetup setup, ITestOutputHelper testOutputHelper)
    {
        _setup = setup;
        _testOutputHelper = testOutputHelper;
        IntegrationTestSetup.InitGlobalLogging(testOutputHelper);
    }
    
    private const string testUserName = "alice";
    private const string testUserPassword = "Password1#";
    private const string clientId = "client";
    private const string clientSecret = "secret";
    private const string apiScope = "api1";
    
    private readonly Action<IdentityConfig> configureIdentity = (config) =>
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
    };
    
    [Fact]
    public async Task Migration_ShouldNotRun_ByDefault()
    {
        // Arrange
        var app = await _setup.Create()
            .Configure(configureIdentity)
            .StartAppAsync();
        
        // Assert
        try
        {
            var dbContext = app.Services.CreateScope().ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            _ = await dbContext.Users.FirstOrDefaultAsync();

            Assert.Fail(
                "Expected exception was not thrown. Database should not be created, thus context.Users should not be accessible.");
        }
        catch (PostgresException ex)
        {
            _testOutputHelper.WriteLine($"Caught expected PostgresException {ex}, database does not exist.");
        }
        catch (Exception ex) when (ex is not FailException)
        {
            Assert.Fail("Unexpected exception type caught: " + ex.GetType());
        }
    }
    
    [Fact]
    public async Task Seeding_ShouldNotRun_ByDefault()
    {
        // Arrange
        var app = await _setup.Create()
            .Configure(configureIdentity)
            .Configure<MigrationsOptions>(options =>
        {
            options.ApplyMigrations = true;
        })
            .StartAppAsync();
        
        // Assert
        var serviceProvider = app.Services.CreateScope().ServiceProvider;
        serviceProvider.ShouldNotBeNull();
        
        var applicationDbContext = serviceProvider
            .GetRequiredService<ApplicationDbContext>();
        applicationDbContext.ShouldNotBeNull();
        Assert.Empty(await applicationDbContext.Users.ToListAsync());
        
        var configurationDbContext = serviceProvider.GetRequiredService<Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext>();
        configurationDbContext.ShouldNotBeNull();
        
        (await configurationDbContext.Clients.ToListAsync()).ShouldBeEmpty("Clients shouldn't be seeded");
        (await configurationDbContext.ApiScopes.ToListAsync()).ShouldBeEmpty("ApiScopes shouldn't be seeded");
        (await configurationDbContext.IdentityResources.ToListAsync()).ShouldBeEmpty("IdentityResources shouldn't be seeded");
    }
    
    // [Fact]
    // public async Task Seeding_ShouldRun_EvenWithoutMigrations()
    // {
    //     // Arrange
    //     // CreateDb
    //     var migrationsHS = new MigrationHostedService(
    //     
    //     setup.Configure(configureIdentity);
    //     setup.Configure<MigrationsOptions>(options =>
    //     {
    //         options.SeedData = true;
    //     });
    //     
    //     // Act
    //     await setup.StartAppAsync(testOutputHelper);
    //     
    //     // Assert
    //     var serviceProvider = setup.app!.Services.CreateScope().ServiceProvider;
    //     serviceProvider.ShouldNotBeNull();
    //     
    //     var applicationDbContext = serviceProvider
    //         .GetRequiredService<ApplicationDbContext>();
    //     applicationDbContext.ShouldNotBeNull();
    //     Assert.Empty(await applicationDbContext.Users.ToListAsync());
    //     
    //     var configurationDbContext = serviceProvider.GetRequiredService<Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext>();
    //     configurationDbContext.ShouldNotBeNull();
    //     
    //     (await configurationDbContext.Clients.ToListAsync()).ShouldBeEmpty("Clients shouldn't be seeded");
    //     (await configurationDbContext.ApiScopes.ToListAsync()).ShouldBeEmpty("ApiScopes shouldn't be seeded");
    //     (await configurationDbContext.IdentityResources.ToListAsync()).ShouldBeEmpty("IdentityResources shouldn't be seeded");
    // }
    //
    // private async Task CreateDatabaseAsync()
    // {
    //     
    // }
}