using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Persistence;

namespace OweMe.Identity.Migrator.Factories;

internal abstract class BaseDbContextFactory<TContext>(ILoggerFactory loggerFactory) : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    public TContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();

        var connectionString = args.Length > 0 ? args[0] : null;
        optionsBuilder.ConfigureDbContextOptions(connectionString);

        var enableVerboseLogging = args.Length > 1 && bool.TryParse(args[1], out var verbose) && verbose;
        if (enableVerboseLogging)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        optionsBuilder.UseLoggerFactory(loggerFactory);
        return CreateInstance(optionsBuilder.Options);
    }

    protected abstract TContext CreateInstance(DbContextOptions<TContext> options);
}
