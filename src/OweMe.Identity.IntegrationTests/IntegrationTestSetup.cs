using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.Server.Setup;
using Serilog;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public class IntegrationTestSetup
{
    public App Create()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("https://[::1]:0");
        
        var app = new App
        {
            Builder = builder
        };
        return app;
    }

    public static void InitGlobalLogging(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateBootstrapLogger();
    }
}

public sealed class App : IAsyncDisposable
{
    public required WebApplicationBuilder Builder { get; set; }

    private PostgreSqlContainer? _postgresContainer;
    
    public App Configure<T>(Action<T> configure)
        where T : class
    {
        Builder.Services.Configure(configure);
        return this;
    }

    public App WithDatabase()
    {
        if (_postgresContainer != null)
        {
            throw new InvalidOperationException("Database container is already created.");
        }

        _postgresContainer = CreatePostgresSqlContainer();
        return this;
    }

    public App WithConnectionString(string connectionString)
    {
        Builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
        return this;
    }
    
    public async Task<WebApplication> StartAppAsync()
    {
        if (_postgresContainer is not null)
        {
            await _postgresContainer.StartAsync();
            _ = WithConnectionString(_postgresContainer.GetConnectionString());
        }
        
        var app = Builder.AddIdentityServer().Build();
        app.ConfigurePipeline();
        await app.StartAsync();
        Log.Information("Started");
        return app;
    }

    public static PostgreSqlContainer CreatePostgresSqlContainer()
    {
        return new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithPortBinding(5432, true)
            .Build();
    }

    public ValueTask DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            return _postgresContainer.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }
}
