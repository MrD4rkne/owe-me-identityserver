using Duende.IdentityServer.Services;
using OweMe.Identity.Persistence;
using OweMe.Identity.Persistence.Users;
using OweMe.Identity.Persistence.Users.Domain;
using OweMe.Identity.Server.Data;
using OweMe.Identity.Server.Users.Application;
using OweMe.Identity.Server.Users.Presentation;

namespace OweMe.Identity.Server.Users;

internal static class DependencyInjection
{
    public static void AddUsers(this WebApplicationBuilder builder)
    {
        // Persistence
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.ConfigureDbContextOptions(builder.Configuration.GetConnectionString(Constants.ConnectionStringName));
        });

        // Application
        builder.Services.AddTransient<IProfileService, ProfileService>();
        builder.Services.AddTransient<IUserService, UserService>();
    }

    public static void UseUsers(this WebApplication app)
    {
        // Endpoints
        app.MapGetUserEndpoint();
    }
}
