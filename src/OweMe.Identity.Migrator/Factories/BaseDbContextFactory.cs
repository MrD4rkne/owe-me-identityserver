using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Persistence;

namespace OweMe.Identity.Migrator.Factories;

internal abstract class BaseDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly ILoggerFactory? _loggerFactory;

    protected BaseDbContextFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    protected BaseDbContextFactory()
    {
    }

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

        if (_loggerFactory is not null)
        {
            optionsBuilder.UseLoggerFactory(_loggerFactory);
        }
        else
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }

        return CreateInstance(optionsBuilder.Options);
    }

    protected abstract TContext CreateInstance(DbContextOptions<TContext> options);
}
