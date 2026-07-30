using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OweMe.Identity.Migrator.Orchiestration;

namespace OweMe.Identity.Migrator.Seeding;

public sealed class SeedCommand(IServiceProvider serviceProvider, ILogger<SeedCommand> logger, IOptions<SeedData> seedData) : ICommand
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting database seeding");

        await SeedScopes(cancellationToken);
        await SeedClients(cancellationToken);

        logger.LogInformation("Seeding finished.");
    }

    private async Task SeedScopes(CancellationToken cancellationToken)
    {
        if(seedData.Value.Scopes.Count == 0)
        {
            logger.LogInformation("No scopes to seed, skipping.");
            return;
        }

        logger.LogDebug("Seeding scopes");
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var scopesToSeed = seedData.Value.Scopes;
        var distinctSeedScopes = scopesToSeed
            .DistinctBy(s => s.Name)
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

    private async Task SeedClients(CancellationToken cancellationToken)
    {
        if (seedData.Value.Clients.Count == 0)
        {
            logger.LogInformation("No clients to seed, skipping.");
            return;
        }

        logger.LogDebug("Seeding clients");
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        if (seedData.Value.Clients.Count == 0)
        {
            logger.LogInformation("No clients to seed, skipping.");
            return;
        }

        var clientsToSeed = seedData.Value.Clients;
        var distinctSeedClients = clientsToSeed
            .GroupBy(c => c.ClientId)
            .Select(g => g.First())
            .ToList();
        var seedClientIds = distinctSeedClients.Select(c => c.ClientId).ToList();

        var existingClientsDict = await context.Clients
            .Where(c => seedClientIds.Contains(c.ClientId))
            .Include(c => c.ClientSecrets)
            .Include(c => c.AllowedGrantTypes)
            .ToDictionaryAsync(c => c.ClientId, cancellationToken);

        var clientsToAdd = new List<Duende.IdentityServer.EntityFramework.Entities.Client>();
        foreach (var seedClient in distinctSeedClients)
        {
            if (existingClientsDict.TryGetValue(seedClient.ClientId, out var existingClient))
            {
                UpdateClient(existingClient, seedClient);
            }
            else
            {
                clientsToAdd.Add(UpdateClient(new() { ClientId = seedClient.ClientId }, seedClient));
            }
        }

        if (clientsToAdd.Count > 0)
        {
            logger.LogDebug("Detected {NewClientsCount} new clients.", clientsToAdd.Count);
            await context.Clients.AddRangeAsync(clientsToAdd, cancellationToken);
        }

        if (context.ChangeTracker.HasChanges())
        {
            logger.LogDebug("Detected changes, saving...");
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private static Duende.IdentityServer.EntityFramework.Entities.Client UpdateClient(Duende.IdentityServer.EntityFramework.Entities.Client existing, Client client)
    {
        existing.ClientName = client.ClientName;
        existing.Description = client.Description;
        existing.AllowedGrantTypes = client.AllowedGrantTypes.Select(gt => new ClientGrantType { GrantType = gt }).ToList();
        existing.AllowedScopes = client.AllowedScopes.Select(s => new ClientScope { Scope = s }).ToList();
        existing.ClientSecrets ??= [];

        var secretsToUpdate = existing.ClientSecrets
            .Join(client.ClientSecrets, s => s.Value, cs => cs.Value, (s, cs) => (s, cs))
            .ToList();
        foreach (var (secretToUpdate, seedSecret) in secretsToUpdate)
        {
            secretToUpdate.Expiration = seedSecret.Expiration?.UtcDateTime;
            secretToUpdate.Description = seedSecret.Description;
        }

        var newSecrets = client.ClientSecrets.Where(s => existing.ClientSecrets.All(c => c.Value != s.Value))
            .Select(s => new ClientSecret
            {
                Value = s.Value,
                Type = s.Type,
                Expiration = s.Expiration?.UtcDateTime,
                Description = s.Description
            });
        existing.ClientSecrets.AddRange(newSecrets);

        return existing;
    }
}
