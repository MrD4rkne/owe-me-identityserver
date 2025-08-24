using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Server.Setup;
using OweMe.Identity.Server.Users.Domain;

namespace OweMe.Identity.Server.Data;

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
        
        await SeedClients(context.Clients, config);
        await SeedIdentityResources(context.IdentityResources, config);
        await SeedApiResources(context.ApiScopes, config);
        
        logger.LogDebug("Saving changes to the database");
        _ = await context.SaveChangesAsync(cancellationToken);
        
        await SeedUsers(config.Users, cancellationToken);
    }

    /// <summary>
    /// Seed the database with test users.
    /// </summary>
    private async Task SeedUsers(IReadOnlyCollection<TestUser> users, CancellationToken cancellationToken = default)
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
                logger.LogDebug("Creating test user {Username}", testUser.UserName);
                
                var result = await userManager.CreateAsync(testUser, user.Password);
                if (result.Succeeded)
                {
                    logger.LogDebug("Test user created");
                }
                else
                {
                    logger.LogError("Test user creation failed: {Result}", result);
                }
            }
            else
            {
                logger.LogWarning("Test user {Username} already exists", testUser.UserName);
            }
        }
    }
    
    private Task SeedClients(DbSet<Client> clients, Config config)
    {
        logger.LogDebug("Seeding Clients");
        var clientsToAdd = config.Clients
            .Select(client => client.ToEntity())
            .Where(clientEntity => !clients.Any(c => c.ClientId == clientEntity.ClientId));
        return clients.AddRangeAsync(clientsToAdd);
    }
    
    private Task SeedIdentityResources(DbSet<IdentityResource> identityResources, Config config)
    {
        logger.LogDebug("Seeding Identity Resources");
        var identityResourcesToAdd = config.IdentityResources
            .Select(resource => resource.ToEntity())
            .Where(resourceEntity => !identityResources.Any(r => r.Name == resourceEntity.Name));
        return identityResources.AddRangeAsync(identityResourcesToAdd);
    }
    
    private Task SeedApiResources(DbSet<ApiScope> scopes, Config config)
    {
        logger.LogDebug("Seeding Api Resources");
        var apiScopesToAdd = config.ApiScopes
            .Select(scope => scope.ToEntity())
            .Where(scopeEntity => !scopes.Any(s => s.Name == scopeEntity.Name));
        return scopes.AddRangeAsync(apiScopesToAdd);
    }
}