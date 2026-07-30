using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Persistence.Users;

namespace OweMe.Identity.Migrator.Migrations.Factories;

internal sealed class ApplicationDbContextFactory : BaseDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContextFactory() {}

    protected override ApplicationDbContext CreateInstance(DbContextOptions<ApplicationDbContext> options)
    {
        return new ApplicationDbContext(options);
    }
}
