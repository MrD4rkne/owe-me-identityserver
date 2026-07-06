using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Persistence.IdentityServer;

namespace OweMe.Identity.Migrator.Factories;

internal sealed class DataProtectionDbContextFactory(ILoggerFactory loggerFactory) : BaseDbContextFactory<DataProtectionDbContext>(loggerFactory)
{
    protected override DataProtectionDbContext CreateInstance(DbContextOptions<DataProtectionDbContext> options)
    {
        return new DataProtectionDbContext(options);
    }
}
