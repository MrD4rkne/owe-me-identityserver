﻿using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using OweMe.Identity.Server.Models;

namespace OweMe.Identity.Server.Setup;

internal sealed class SeedData
{
    /// <summary>
    /// Seed the database with initial data from the configuration.
    /// </summary>
    /// <param name="app"></param>
    internal static async Task InitializeDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();

        var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        
        logger.LogInformation("Seeding database with identity config");
        
        logger.LogInformation("Seeding Clients");
        if (!context.Clients.Any())
        {
            foreach (var client in Config.Clients)
            {
                context.Clients.Add(client.ToEntity());
            }
            
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Seeding Identity Resources");
        if (!context.IdentityResources.Any())
        {
            foreach (var resource in Config.IdentityResources)
            {
                context.IdentityResources.Add(resource.ToEntity());
            }
            
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Seeding Api Resources");
        if (!context.ApiScopes.Any())
        {
            foreach (var resource in Config.ApiScopes)
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
    internal static async Task SeedUsers(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();
        
        logger.LogInformation("Seeding database with test users");

        foreach (var user in Config.Users)
        {
            var testUser = new ApplicationUser
            {
                UserName = user.Username,
                Email = user.Username,
                EmailConfirmed = true
            };

            if (! userManager.Users.Any(u => u.UserName != testUser.UserName))
            {
                logger.LogInformation("Creating test user {username}", testUser.UserName);
                
                var result = await userManager.CreateAsync(testUser, user.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("Test user created");
                }
                else
                {
                    logger.LogError("Test user creation failed: {errors}", result.Errors);
                }
            }
            else
            {
                logger.LogInformation("Test user {username} already exists", testUser.UserName);
            }
        }
    }
}