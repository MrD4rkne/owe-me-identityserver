using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OweMe.Identity.Persistence;

namespace OweMe.Identity.Migrator.Migrations.Factories;

internal abstract class BaseDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    public TContext CreateDbContext(string[] args)
    {
        return CreateDbContext((string?)null);
    }

    internal TContext CreateDbContext(string? connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.ConfigureDbContextOptions(connectionString);
        return CreateInstance(optionsBuilder.Options);
    }

    protected abstract TContext CreateInstance(DbContextOptions<TContext> options);
}
