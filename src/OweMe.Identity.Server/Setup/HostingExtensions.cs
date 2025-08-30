using System.Diagnostics.CodeAnalysis;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Server.Data;
using OweMe.Identity.Server.Users;
using OweMe.Identity.Server.Users.Application;
using OweMe.Identity.Server.Users.Domain;
using OweMe.Identity.Server.Users.Persistence;
using Serilog;

namespace OweMe.Identity.Server.Setup;

[ExcludeFromCodeCoverage]
public static class HostingExtensions
{
    public static WebApplicationBuilder AddIdentityServer(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog();
        
        builder.Services.AddRazorPages();
        
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        builder.Services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = dbContextBuilder =>
                {
                    dbContextBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                };
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = dbContextBuilder =>
                {
                    dbContextBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                };

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
            })
            .AddAspNetIdentity<ApplicationUser>();
        
        builder.AddUsers();
        builder.Services.AddSingleton<DatabaseSeeder>();
        builder.Services.AddHostedService<MigrationHostedService>();
        builder.Services.AddOptions<IdentityConfig>().BindConfiguration(IdentityConfig.SectionName);
        builder.Services.AddOptions<MigrationsOptions>().BindConfiguration(MigrationsOptions.SectionName);

        return builder;
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
        
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
            
        app.UseIdentityServer();
        
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
