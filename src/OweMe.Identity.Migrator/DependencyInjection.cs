using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator.Migrations;
using OweMe.Identity.Migrator.Seeding;
using OweMe.Identity.Persistence;

namespace OweMe.Identity.Migrator;

internal static class DependencyInjection
{
    internal static IServiceProvider CreateProvider(IConfiguration configuration, bool isVerbose)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.Configure<SeedData>(configuration.GetSection(SeedData.SectionName));

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

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new MissingConnectionStringException();
        }
        services.AddOweMeStorage(connectionString);

        services.AddTransient<MigrateCommand>();
        services.AddTransient<SeedCommand>();

        return services.BuildServiceProvider();
    }
}
