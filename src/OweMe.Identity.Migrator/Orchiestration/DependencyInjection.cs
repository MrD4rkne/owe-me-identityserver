using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OweMe.Identity.Persistence;

namespace OweMe.Identity.Migrator.Orchiestration;

internal static class DependencyInjection
{
    internal static IServiceCollection CreateProvider(IConfiguration configuration, bool isVerbose)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);

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

        return services;
    }

    /// <summary>
    /// Run validation of all Options. Necessary as we're not using a standard builder.
    /// </summary>
    internal static IServiceProvider ValidateOptions(this IServiceProvider provider)
    {
        var validator = provider.GetService<IStartupValidator>();
        validator?.Validate();
        return provider;
    }
}
