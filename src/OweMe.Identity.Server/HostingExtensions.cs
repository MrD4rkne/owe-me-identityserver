using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Domain;
using OweMe.Identity.Server.Models;
using OweMe.Identity.Server.Services;
using OweMe.Identity.Server.Setup;
using Serilog;

namespace OweMe.Identity.Server;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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
                    dbContextBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                };
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = dbContextBuilder =>
                {
                    dbContextBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                };

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
            })
            .AddTestUsers(Config.Users)
            .AddAspNetIdentity<ApplicationUser>();
        
        builder.Services.AddTransient<IProfileService, ProfileService>();

        return builder.Build();
    }
    
    public static async Task<WebApplication> ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
        
        await SeedData.InitializeDatabase(app);
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            
            await SeedData.SeedUsers(app);
        }
            
        app.UseIdentityServer();

        return app;
    }
}
