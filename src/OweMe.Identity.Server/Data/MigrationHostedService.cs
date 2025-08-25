using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OweMe.Identity.Server.Setup;
using OweMe.Identity.Server.Users.Persistence;

namespace OweMe.Identity.Server.Data;

public class MigrationHostedService(
    IServiceProvider serviceProvider,
    ILogger<MigrationHostedService> logger,
    DatabaseSeeder seeder,
    IOptions<MigrationsOptions> migrationsOptions)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting database migration service");
        using var scope = serviceProvider.CreateScope();

        if (ShouldRunMigrations())
        {
            await RunMigrationsAsync(scope, cancellationToken);
        }
        else
        {
            logger.LogInformation("Skipping migrations");
        }
        
        if (ShouldSeedData())
        {
            await seeder.InitializeDatabase(cancellationToken);
        }
        else
        {
            logger.LogInformation("Skipping seeding data");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    private async Task RunMigrationsAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying database migrations");
        
        await MigrateContextAsync<ConfigurationDbContext>(scope, logger, cancellationToken);
        await MigrateContextAsync<PersistedGrantDbContext>(scope, logger, cancellationToken);
        await MigrateContextAsync<ApplicationDbContext>(scope, logger, cancellationToken);
    }
    
    private static Task MigrateContextAsync<TContext>(IServiceScope scope, ILogger logger, CancellationToken cancellationToken)
    where TContext : DbContext
    {
        logger.LogDebug("Migrating {DbContext}", typeof(TContext).Name);
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        return dbContext.Database.MigrateAsync(cancellationToken);
    }
    
    private bool ShouldRunMigrations()
    {
        return migrationsOptions.Value.ApplyMigrations;
    }
    
    private bool ShouldSeedData()
    {
        return migrationsOptions.Value.SeedData;
    }
}