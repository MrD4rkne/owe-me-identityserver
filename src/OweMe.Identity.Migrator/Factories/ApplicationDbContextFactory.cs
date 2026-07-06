using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Persistence.Users;

namespace OweMe.Identity.Migrator.Factories;

internal sealed class ApplicationDbContextFactory(ILoggerFactory loggerFactory) : BaseDbContextFactory<ApplicationDbContext>(loggerFactory)
{
    protected override ApplicationDbContext CreateInstance(DbContextOptions<ApplicationDbContext> options)
    {
        return new ApplicationDbContext(options);
    }
}
