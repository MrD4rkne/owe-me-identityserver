using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using OweMe.Identity.Server.Users.Persistence;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OweMe.Identity.IntegrationTests.Setup;

public sealed class MigrationSeedingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MigrationSeedingTests(ITestOutputHelper testOutputHelper)
    {
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
            new TestUser
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
        var app = await IntegrationTestSetup.Create()
            .Configure(configureIdentity)
            .WithDatabase()
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
        var app = await IntegrationTestSetup.Create()
            .Configure(configureIdentity)
            .Configure<MigrationsOptions>(options =>
        {
            options.ApplyMigrations = true;
        })
            .WithDatabase()
            .StartAppAsync();
        
        // Assert
        var serviceProvider = app.Services.CreateScope().ServiceProvider;
        serviceProvider.ShouldNotBeNull();
        
        var applicationDbContext = serviceProvider
            .GetRequiredService<ApplicationDbContext>();
        applicationDbContext.ShouldNotBeNull();
        Assert.Empty(await applicationDbContext.Users.ToListAsync());

        var configurationDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
        configurationDbContext.ShouldNotBeNull();
        
        (await configurationDbContext.Clients.ToListAsync()).ShouldBeEmpty("Clients shouldn't be seeded");
        (await configurationDbContext.ApiScopes.ToListAsync()).ShouldBeEmpty("ApiScopes shouldn't be seeded");
        (await configurationDbContext.IdentityResources.ToListAsync()).ShouldBeEmpty("IdentityResources shouldn't be seeded");
    }

    [Fact]
    public async Task Seeding_ShouldRun_EvenWithoutMigrations()
    {
        // Arrange
        var postgresContainer = AppBuilder.CreatePostgresSqlContainer();
        await postgresContainer.StartAsync();

        // Let's create the database with migrations, but without seeding
        _ = await IntegrationTestSetup.Create()
            .Configure<MigrationsOptions>(options =>
            {
                options.ApplyMigrations = true;
                options.SeedData = false;
            })
            .WithConnectionString(postgresContainer.GetConnectionString())
            .StartAppAsync();

        // Assert
        var app = await IntegrationTestSetup.Create()
            .Configure(configureIdentity)
            .Configure<MigrationsOptions>(options =>
            {
                options.ApplyMigrations = false;
                options.SeedData = true;
            })
            .WithConnectionString(postgresContainer.GetConnectionString())
            .StartAppAsync();

        var serviceProvider = app.Services.CreateScope().ServiceProvider;
        AssertSeedingWorked(serviceProvider);
    }

    [Fact]
    public async Task SeedingAndMigrations_ShouldRun_WhenEnabled()
    {
        // Arrange
        var app = await IntegrationTestSetup.Create()
            .Configure(configureIdentity)
            .Configure<MigrationsOptions>(options =>
            {
                options.ApplyMigrations = true;
                options.SeedData = true;
            })
            .WithDatabase()
            .StartAppAsync();

        // Assert
        var serviceProvider = app.Services.CreateScope().ServiceProvider;
        AssertSeedingWorked(serviceProvider);
    }

    private static void AssertSeedingWorked(IServiceProvider serviceProvider)
    {
        var applicationDbContext = serviceProvider
            .GetRequiredService<ApplicationDbContext>();
        applicationDbContext.ShouldNotBeNull();
        var users = applicationDbContext.Users.ToListAsync().Result;
        users.Count.ShouldBe(1);
        Assert.Equal(testUserName, users[0].UserName);

        var configurationDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
        configurationDbContext.ShouldNotBeNull();

        var clients = configurationDbContext.Clients
            .Include(c => c.AllowedGrantTypes)
            .Include(c => c.ClientSecrets)
            .Include(c => c.AllowedScopes)
            .ToListAsync().Result;
        clients.Count.ShouldBe(1);
        clients[0].ClientId.ShouldBe(clientId);
        clients[0].ClientSecrets.ShouldNotBeEmpty();

        clients[0].AllowedGrantTypes.Count.ShouldBe(GrantTypes.ResourceOwnerPassword.Count);
        clients[0].AllowedGrantTypes.ShouldAllBe(grant => GrantTypes.ResourceOwnerPassword.Contains(grant.GrantType));

        clients[0].AllowedScopes.Count.ShouldBe(1);
        clients[0].AllowedScopes[0].Scope.ShouldBe(apiScope);

        var apiScopes = configurationDbContext.ApiScopes.ToListAsync().Result;
        apiScopes.Count.ShouldBe(1);
        Assert.Equal(apiScope, apiScopes[0].Name);

        var identityResources = configurationDbContext.IdentityResources.ToListAsync().Result;
        identityResources.Count.ShouldBe(3);
        identityResources.ShouldContain(resource => resource.Name == "openid");
        identityResources.ShouldContain(resource => resource.Name == "profile");
        identityResources.ShouldContain(resource => resource.Name == "user");
    }
}