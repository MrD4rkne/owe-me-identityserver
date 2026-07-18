using Duende.IdentityServer.Services;
using OweMe.Identity.Persistence.Users.Domain;
using OweMe.Identity.Server.Users.Application;
using OweMe.Identity.Server.Users.Presentation;

namespace OweMe.Identity.Server.Users;

internal static class DependencyInjection
{
    public static void AddUsers(this WebApplicationBuilder builder)
    {
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
