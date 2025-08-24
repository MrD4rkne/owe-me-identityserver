using System.Diagnostics.CodeAnalysis;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Identity;
using OweMe.Identity.Server.Setup;

namespace OweMe.Identity.Server.Users.Persistence;

[ExcludeFromCodeCoverage]
public sealed class SeedData
{
    /// <summary>
    /// Seed the database with initial data from the configuration.
    /// </summary>
    /// <param name="applicationBuilder">App's builder</param>
    /// <param name="config">Configuration</param>
    internal static async Task InitializeDatabase(IApplicationBuilder applicationBuilder, Config config)
    {
        using var serviceScope =
            applicationBuilder.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();

        var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

        logger.LogInformation("Seeding database with identity config");

        if (!context.Clients.Any())
        {
            logger.LogInformation("Seeding Clients");
            foreach (var client in config.Clients)
            {
                context.Clients.Add(client.ToEntity());
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Clients seeded");
        }
        else
        {
            logger.LogInformation("Clients already seeded");
        }

        logger.LogInformation("Seeding Identity Resources");
        if (!context.IdentityResources.Any())
        {
            foreach (var resource in config.IdentityResources)
            {
                context.IdentityResources.Add(resource.ToEntity());
            }

            await context.SaveChangesAsync();
        }

        logger.LogInformation("Seeding Api Resources");
        if (!context.ApiScopes.Any())
        {
            foreach (var resource in config.ApiScopes)
            {
                context.ApiScopes.Add(resource.ToEntity());
            }

            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed the database with test users.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="users"></param>
    internal static async Task SeedUsers(IApplicationBuilder app, IReadOnlyCollection<TestUser> users)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();
        
        logger.LogInformation("Seeding database with test users");

        foreach (var user in users)
        {
            var testUser = new ApplicationUser
            {
                UserName = user.Username,
                Email = user.Username,
                EmailConfirmed = true
            };

            if (! userManager.Users.Any(u => u.UserName != testUser.UserName))
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
}