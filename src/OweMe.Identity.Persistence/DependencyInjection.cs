using Duende.IdentityServer.EntityFramework.Storage;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.Persistence.IdentityServer;
using OweMe.Identity.Persistence.Users;

namespace OweMe.Identity.Persistence;

public static class ServiceCollectionPersistenceExtensions
{
    public static IServiceCollection AddOweMeStorage(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(opts => opts.ConfigureDbContextOptions(connectionString));
        services.AddDbContext<DataProtectionDbContext>(opts => opts.ConfigureDbContextOptions(connectionString));

        services.AddConfigurationDbContext(opts =>
            opts.ConfigureDbContext = b => b.ConfigureDbContextOptions(connectionString));

        services.AddOperationalDbContext(opts =>
            opts.ConfigureDbContext = b => b.ConfigureDbContextOptions(connectionString));

        return services;
    }
}
