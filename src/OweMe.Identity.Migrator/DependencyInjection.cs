using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Persistence;

namespace OweMe.Identity.Migrator;

internal static class DependencyInjection
{
    internal static IServiceProvider CreateProvider(string? connectionString, bool isVerbose)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "[HH:mm:ss] ";
                options.IncludeScopes = false;
            });

            if (isVerbose)
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.Information);
            }
            else
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            }
        });

        services.AddOweMeStorage(connectionString);

        services.AddTransient<MigrateCommand>();

        return services.BuildServiceProvider();
    }
}
