using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Persistence.Users;

namespace OweMe.Identity.Migrator.Factories;

internal sealed class ApplicationDbContextFactory : BaseDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContextFactory(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    public ApplicationDbContextFactory() {}

    protected override ApplicationDbContext CreateInstance(DbContextOptions<ApplicationDbContext> options)
    {
        return new ApplicationDbContext(options);
    }
}
