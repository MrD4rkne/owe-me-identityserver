using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace OweMe.Identity.Migrator.Migrations.Factories;

internal sealed class ConfigurationDbContextFactory: BaseDbContextFactory<ConfigurationDbContext>
{
    public ConfigurationDbContextFactory() {}

    protected override ConfigurationDbContext CreateInstance(DbContextOptions<ConfigurationDbContext> options)
    {
        var context = new ConfigurationDbContext(options);
        context.StoreOptions = new ConfigurationStoreOptions();
        return context;
    }
}
