using Microsoft.EntityFrameworkCore;

namespace OweMe.Identity.Persistence;

public static class DbContextSharingExtensions
{
    public static DbContextOptionsBuilder ConfigureDbContextOptions(
        this DbContextOptionsBuilder options,
        string? connectionString)
    {
        options.UseNpgsql(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(DbContextSharingExtensions).Assembly.GetName().Name);
        });

        return options;
    }
}
