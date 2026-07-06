using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator.Factories;
using OweMe.Identity.Persistence.IdentityServer;
using OweMe.Identity.Persistence.Users;

namespace OweMe.Identity.Migrator;

internal sealed class MigrateCommand(ILoggerFactory loggerFactory)
{
    public async Task ExecuteAsync(string connectionString, CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger<MigrateCommand>();
        logger.LogInformation("Starting Database Migrations ...");

        var factoryArgs = new[] { connectionString};
        await MigrateContext<ApplicationDbContextFactory, ApplicationDbContext>(factoryArgs, cancellationToken);
        await MigrateContext<PersistedGrantDbContextFactory, PersistedGrantDbContext>(factoryArgs, cancellationToken);
        await MigrateContext<ConfigurationDbContextFactory, ConfigurationDbContext>(factoryArgs, cancellationToken);
        await MigrateContext<DataProtectionDbContextFactory, DataProtectionDbContext>(factoryArgs, cancellationToken);

        logger.LogInformation("All database schemas successfully upgraded!");
    }

    private async Task MigrateContext<TContextFactory, TContext>(string[] factoryArgs, CancellationToken cancellationToken)
        where TContextFactory : BaseDbContextFactory<TContext> where TContext : DbContext
    {
        var logger = loggerFactory.CreateLogger($"Migration of {typeof(TContext).Name}");

        logger.LogInformation("Migrating Schema for {Context}...", typeof(TContext).Name);

        logger.LogDebug("Creating DbContextFactory for {Context}...", typeof(TContextFactory).Name);
        var factory = (TContextFactory)Activator.CreateInstance(typeof(TContextFactory), loggerFactory)!;
        logger.LogDebug("DbContextFactory for {Context} created successfully.", typeof(TContextFactory).Name);

        logger.LogDebug("Creating DbContext for {Context}...", typeof(TContext).Name);
        await using var context = factory.CreateDbContext(factoryArgs);
        logger.LogDebug("DbContext for {Context} created successfully.", typeof(TContext).Name);

        logger.LogDebug("Applying migrations for {Context}...", typeof(TContext).Name);
        await context.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migration of {Context} completed successfully.", typeof(TContext).Name);
    }
}
