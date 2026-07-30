using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace OweMe.Identity.Migrator.Migrations.Factories;

internal sealed class PersistedGrantDbContextFactory : BaseDbContextFactory<PersistedGrantDbContext>
{
    public PersistedGrantDbContextFactory() {}

    protected override PersistedGrantDbContext CreateInstance(DbContextOptions<PersistedGrantDbContext> options)
    {
        var context = new PersistedGrantDbContext(options);
        context.StoreOptions = new OperationalStoreOptions();
        return context;
    }
}
