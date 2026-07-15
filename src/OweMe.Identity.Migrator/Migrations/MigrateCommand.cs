using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator.Orchiestration;
using OweMe.Identity.Persistence.IdentityServer;
using OweMe.Identity.Persistence.Users;

namespace OweMe.Identity.Migrator.Migrations;

public sealed class MigrateCommand(IServiceProvider serviceProvider, ILogger<MigrateCommand> logger) : ICommand
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Database Migrations ...");

        await MigrateContext<ApplicationDbContext>(cancellationToken);
        await MigrateContext<PersistedGrantDbContext>(cancellationToken);
        await MigrateContext<ConfigurationDbContext>(cancellationToken);
        await MigrateContext<DataProtectionDbContext>(cancellationToken);

        logger.LogInformation("All database schemas successfully upgraded!");
    }

    private async Task MigrateContext<TContext>(CancellationToken cancellationToken)
       where TContext : DbContext
    {
        await using var context = serviceProvider.GetRequiredService<TContext>();

        logger.LogDebug("Applying migrations for {Context}...", typeof(TContext).Name);
        await context.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migration of {Context} completed successfully.", typeof(TContext).Name);
    }
}
