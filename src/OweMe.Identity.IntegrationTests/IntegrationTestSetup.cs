using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.Server.Setup;
using Serilog;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public class IntegrationTestSetup : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5432, true)
        .Build();
    
    private readonly List<App> apps = [];

    public App Create()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("https://[::1]:0");
        
        builder.Configuration["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString();
        
        var app = new App
        {
            Builder = builder
        };
        apps.Add(app);
        return app;
    }

    public static void InitGlobalLogging(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateBootstrapLogger();
    }

    public Task InitializeAsync()
    {
        return _postgresContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgresContainer.DisposeAsync().AsTask();
    }
}

public sealed record App
{
    public required WebApplicationBuilder Builder { get; init; }
    
    public App Configure<T>(Action<T> configure)
        where T : class
    {
        Builder.Services.Configure(configure);
        return this;
    }
    
    public async Task<WebApplication> StartAppAsync()
    {
        Log.Information("Starting up");
        var app = Builder.AddIdentityServer().Build();
        app.ConfigurePipeline();
        await app.StartAsync();
        Log.Information("Started");
        return app;
    }
}
