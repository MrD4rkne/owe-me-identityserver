using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OweMe.Identity.IntegrationTests.Helpers;
using OweMe.Identity.Server.Setup;
using OweMe.Identity.Server.Users.Persistence;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OweMe.Identity.IntegrationTests.Setup;

public sealed class MigrationSeedingTests(ITestOutputHelper outputHelper)
    : TestWithLoggingBase(outputHelper), IAsyncLifetime
{
    private const string testUserName = "alice";
    private const string testUserPassword = "Password1#";
    private const string clientId = "client";
    private const string clientSecret = "secret";
    private const string apiScope = "api1";

    private static readonly Action<IdentityConfig> ConfigureIdentity = config =>
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
    };

    private WebApplicationFactory<Program>? _programFixture;

    public async Task InitializeAsync()
    {
        var programFixture = new ProgramFixture();
        await programFixture.InitializeAsync();

        _programFixture = programFixture
            .WithWebHostBuilder(builder => { builder.WithConfigure(ConfigureIdentity); });
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Migration_ShouldNotRun_ByDefault()
    {
        // Assert
        try
        {
            using var scope = _programFixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            _ = await dbContext.Users.FirstOrDefaultAsync();

            Assert.Fail(
                "Expected exception was not thrown. Database should not be created, thus context.Users should not be accessible.");
        }
        catch (PostgresException ex)
        {
            OutputHelper.WriteLine($"Caught expected PostgresException {ex}, database does not exist.");
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
        var factory = _programFixture.WithWebHostBuilder(builder =>
        {
            builder.WithConfigure<MigrationsOptions>(options =>
            {
                options.ApplyMigrations = true;
                options.SeedData = false;
            });
        });

        // Assert
        var serviceProvider = factory.Services.CreateScope().ServiceProvider;
        serviceProvider.ShouldNotBeNull();

        var applicationDbContext = serviceProvider
            .GetRequiredService<ApplicationDbContext>();
        applicationDbContext.ShouldNotBeNull();
        Assert.Empty(await applicationDbContext.Users.ToListAsync());

        var configurationDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
        configurationDbContext.ShouldNotBeNull();

        (await configurationDbContext.Clients.ToListAsync()).ShouldBeEmpty("Clients shouldn't be seeded");
        (await configurationDbContext.ApiScopes.ToListAsync()).ShouldBeEmpty("ApiScopes shouldn't be seeded");
        (await configurationDbContext.IdentityResources.ToListAsync()).ShouldBeEmpty(
            "IdentityResources shouldn't be seeded");
    }

    [Fact]
    public async Task Seeding_ShouldRun_EvenWithoutMigrations()
    {
        // Arrange
        await using var databaseContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithPortBinding(5432, true)
            .Build();
        await databaseContainer.StartAsync();
        var connectionString = databaseContainer.GetConnectionString();

        // Let's create the database with migrations, but without seeding
        await _programFixture.WithWebHostBuilder(builder =>
            {
                builder.WithConfigure<MigrationsOptions>(options =>
                    {
                        options.ApplyMigrations = true;
                        options.SeedData = false;
                    })
                    .WithConnectionString(connectionString);
            })
            .EnsureInitialized();

        // Act
        var secondApp = _programFixture.WithWebHostBuilder(builder =>
        {
            builder.WithConfigure<MigrationsOptions>(options =>
                {
                    options.ApplyMigrations = false;
                    options.SeedData = true;
                })
                .WithConnectionString(connectionString);
        });

        await secondApp.EnsureInitialized();

        // Assert
        using var scope = secondApp.Services.CreateScope();
        AssertSeedingWorked(scope.ServiceProvider);
    }

    [Fact]
    public async Task SeedingAndMigrations_ShouldRun_WhenEnabled()
    {
        // Arrange
        var app = _ = _programFixture.WithWebHostBuilder(builder =>
        {
            builder.WithConfigure<MigrationsOptions>(options =>
            {
                options.ApplyMigrations = true;
                options.SeedData = true;
            });
        });

        // Act
        await app.EnsureInitialized();

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
            .ToList();
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
