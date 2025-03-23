using System.Diagnostics.CodeAnalysis;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Server.Data;
using OweMe.Identity.Server.Users;
using Serilog;

namespace OweMe.Identity.Server.Setup;

[ExcludeFromCodeCoverage]
internal static class HostingExtensions
{
    public static async Task<WebApplication> ConfigureServices(this WebApplicationBuilder builder, Config config)
    {
        builder.Services.AddSerilog();
        
        builder.Services.AddRazorPages();
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        
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
        
        builder.Services.AddTransient<IProfileService, ProfileService>();
        
        if(builder.Configuration["Migrations:Apply"] == "true")
        {
            Log.Information("Applying migrations");
            await builder.Services.ApplyMigrations<ApplicationDbContext>();
            await builder.Services.ApplyMigrations<ConfigurationDbContext>();
            await builder.Services.ApplyMigrations<PersistedGrantDbContext>();
        }

        return builder.Build();
    }
    
    public static async Task ApplyMigrations<TContext>(this IServiceCollection services)
    where TContext : DbContext
    {
        Log.Information("Applying migrations for {Context}", typeof(TContext).Name);
        
        using var scope = services.BuildServiceProvider().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await dbContext.Database.MigrateAsync();
    }
    
    public static async Task<WebApplication> ConfigurePipeline(this WebApplication app, Config config)
    { 
        app.UseSerilogRequestLogging();
        
        await SeedData.InitializeDatabase(app, config);
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            
            await SeedData.SeedUsers(app, config.Users);
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
