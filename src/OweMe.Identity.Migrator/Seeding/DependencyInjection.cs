using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OweMe.Identity.Migrator.Seeding;

internal static class DependencyInjection
{
    internal static IServiceCollection AddSeedCommand(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SeedData>()
            .Bind(configuration.GetSection(SeedData.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
