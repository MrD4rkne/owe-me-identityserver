using System.Diagnostics.CodeAnalysis;
using Duende.IdentityServer.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using OweMe.Identity.Persistence.IdentityServer;
using OweMe.Identity.Persistence.Users;
using OweMe.Identity.Persistence.Users.Domain;
using OweMe.Identity.Server.Data;
using OweMe.Identity.Server.Users;

namespace OweMe.Identity.Server;

[ExcludeFromCodeCoverage]
public static class HostingExtensions
{
    /// <summary>
    /// B
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddConnectionStringFromEnv(this WebApplicationBuilder builder)
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var username = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var database = Environment.GetEnvironmentVariable("DB_NAME");

        if (builder.Configuration.GetConnectionString(Constants.ConnectionStringName) is null)
        {
            var connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = int.Parse(port),
                Username = username,
                Password = password,
                Database = database,
            }.ToString();
            var data = new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{Constants.ConnectionStringName}"] = connectionString
            };
            builder.Configuration.AddInMemoryCollection(data);
        }

        return builder;
    }

    public static WebApplicationBuilder AddIdentityServer(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddOptions<IdentityServerOptions>()
            .Configure((options) =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
            });

        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<DataProtectionDbContext>();

        var identityServerBuilder = builder.Services.AddIdentityServer(options =>
            {
                // Premium feature, not available for free.
                options.KeyManagement.Enabled = false;
            })
            .AddConfigurationStore()
            .AddOperationalStore(options =>
            {
                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
            })
            .AddAspNetIdentity<ApplicationUser>();
        if (builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }

        builder.AddUsers();

        builder.Services.AddLocalApiAuthentication();

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.UseUsers();
        app.UseIdentityServer();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
