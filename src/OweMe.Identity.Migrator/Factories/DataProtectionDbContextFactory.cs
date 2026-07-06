using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Persistence.IdentityServer;

namespace OweMe.Identity.Migrator.Factories;

internal sealed class DataProtectionDbContextFactory : BaseDbContextFactory<DataProtectionDbContext>
{
    internal DataProtectionDbContextFactory(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    public DataProtectionDbContextFactory() {}

    protected override DataProtectionDbContext CreateInstance(DbContextOptions<DataProtectionDbContext> options)
    {
        return new DataProtectionDbContext(options);
    }
}
