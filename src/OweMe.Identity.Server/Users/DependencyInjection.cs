using Duende.IdentityServer.Services;
using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Server.Users.Application;
using OweMe.Identity.Server.Users.Persistence;

namespace OweMe.Identity.Server.Users;

internal static class DependencyInjection
{
    public static void AddUsers(this WebApplicationBuilder builder)
    {
        // Persistence
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
     
        // Application
        builder.Services.AddTransient<IProfileService, ProfileService>();
    }
}