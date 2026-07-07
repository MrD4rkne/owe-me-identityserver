using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OweMe.Identity.Persistence;

namespace OweMe.Identity.Migrator.Factories;

internal abstract class BaseDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    protected BaseDbContextFactory()
    {
    }

    public TContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.ConfigureDbContextOptions(null);
        return CreateInstance(optionsBuilder.Options);
    }

    protected abstract TContext CreateInstance(DbContextOptions<TContext> options);
}
