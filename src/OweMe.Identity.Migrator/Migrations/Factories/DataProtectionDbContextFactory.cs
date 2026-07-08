using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Persistence.IdentityServer;

namespace OweMe.Identity.Migrator.Migrations.Factories;

internal sealed class DataProtectionDbContextFactory : BaseDbContextFactory<DataProtectionDbContext>
{
    public DataProtectionDbContextFactory() {}

    protected override DataProtectionDbContext CreateInstance(DbContextOptions<DataProtectionDbContext> options)
    {
        return new DataProtectionDbContext(options);
    }
}
