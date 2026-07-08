using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OweMe.Identity.Migrator.Seeding;

internal sealed class SeedCommand(IServiceProvider serviceProvider, ILogger<SeedCommand> logger, IOptions<SeedData> seedData) : ICommand
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting database seeding");

        await SeedScopes(cancellationToken);

        logger.LogInformation("Seeding finished.");
    }

    private async Task SeedScopes(CancellationToken cancellationToken)
    {
        logger.LogDebug("Seeding database scopes");
        await using var context = serviceProvider.GetRequiredService<ConfigurationDbContext>();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var scopesToSeed = seedData.Value.Scopes;
        var distinctSeedScopes = scopesToSeed
            .GroupBy(s => s.Name)
            .Select(g => g.First())
            .ToList();
        var seedScopeNames = distinctSeedScopes.Select(s => s.Name).ToList();

        var existingScopesDict = await context.ApiScopes
            .Where(s => seedScopeNames.Contains(s.Name))
            .ToDictionaryAsync(s => s.Name, cancellationToken);

        var apiScopesToAdd = new List<ApiScope>();
        foreach (var seedScope in distinctSeedScopes)
        {
            if (existingScopesDict.TryGetValue(seedScope.Name, out var existingScope))
            {
                if (existingScope.DisplayName == seedScope.DisplayName &&
                    existingScope.Description == seedScope.Description)
                {
                    continue;
                }
                existingScope.DisplayName = seedScope.DisplayName;
                existingScope.Description = seedScope.Description;
            }
            else
            {
                apiScopesToAdd.Add(new ApiScope
                {
                    Name = seedScope.Name,
                    DisplayName = seedScope.DisplayName,
                    Description = seedScope.Description,
                });
            }
        }

        if (apiScopesToAdd.Count > 0)
        {
            logger.LogDebug("Detected {NewApiScopesCount} new API scopes.", apiScopesToAdd.Count);
            await context.ApiScopes.AddRangeAsync(apiScopesToAdd, cancellationToken);
        }

        if (context.ChangeTracker.HasChanges())
        {
            logger.LogDebug("Detected changes, saving...");
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
