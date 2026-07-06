using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace OweMe.Identity.Migrator.Factories;

internal sealed class ConfigurationDbContextFactory(ILoggerFactory loggerFactory) : BaseDbContextFactory<ConfigurationDbContext>(loggerFactory)
{
    protected override ConfigurationDbContext CreateInstance(DbContextOptions<ConfigurationDbContext> options)
    {
        var context = new ConfigurationDbContext(options);
        context.StoreOptions = new ConfigurationStoreOptions();
        return context;
    }
}
