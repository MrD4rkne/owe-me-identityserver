using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace OweMe.Identity.Migrator.Factories;

internal sealed class PersistedGrantDbContextFactory : BaseDbContextFactory<PersistedGrantDbContext>
{
    public PersistedGrantDbContextFactory(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    public PersistedGrantDbContextFactory() {}

    protected override PersistedGrantDbContext CreateInstance(DbContextOptions<PersistedGrantDbContext> options)
    {
        var context = new PersistedGrantDbContext(options);
        context.StoreOptions = new OperationalStoreOptions();
        return context;
    }
}
