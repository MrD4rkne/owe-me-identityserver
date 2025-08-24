using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Server.Setup;
using OweMe.Identity.Server.Users.Domain;

namespace OweMe.Identity.Server.Users.Persistence;

public sealed class DatabaseSeeder(IServiceScopeFactory serviceScopeFactory,
    ILogger<DatabaseSeeder> logger)
{
    /// <summary>
    /// Seed the database with initial data from the configuration.
    /// </summary>
    internal async Task InitializeDatabase(Config config, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Seeding database with identity config");
        
        using var serviceScope = serviceScopeFactory.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        
        await SeedClients(context, config, cancellationToken);
        await SeedIdentityResources(context, config, cancellationToken);
        await SeedApiResources(context, config, cancellationToken);
    }

    /// <summary>
    /// Seed the database with test users.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="users"></param>
    internal async Task SeedUsers(IReadOnlyCollection<TestUser> users, CancellationToken cancellationToken = default)
    {
        using var serviceScope = serviceScopeFactory.CreateScope();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        logger.LogInformation("Seeding database with test users");

        foreach (var user in users)
        {
            var testUser = new ApplicationUser
            {
                UserName = user.Username,
                Email = user.Username,
                EmailConfirmed = true
            };

            if (! await userManager.Users.AnyAsync(u => u.UserName != testUser.UserName, cancellationToken))
            {
                logger.LogInformation("Creating test user {Username}", testUser.UserName);
                
                var result = await userManager.CreateAsync(testUser, user.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("Test user created");
                }
                else
                {
                    logger.LogError("Test user creation failed: {Result}", result);
                }
            }
            else
            {
                logger.LogInformation("Test user {Username} already exists", testUser.UserName);
            }
        }
    }
    
    private async Task SeedClients(ConfigurationDbContext context, Config config, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Seeding Clients");
        foreach (var client in config.Clients)
        {
            context.Clients.Add(client.ToEntity());
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogDebug("Clients seeded");
    }
    
    private async Task SeedIdentityResources(ConfigurationDbContext context, Config config, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Seeding Identity Resources");
        foreach (var resource in config.IdentityResources)
        {
            context.IdentityResources.Add(resource.ToEntity());
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogDebug("Identity Resources seeded");
    }
    
    private async Task SeedApiResources(ConfigurationDbContext context, Config config, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Seeding Api Resources");
        foreach (var resource in config.ApiScopes)
        {
            context.ApiScopes.Add(resource.ToEntity());
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogDebug("Api Resources seeded");
    }
}