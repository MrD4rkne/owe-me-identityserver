using Duende.IdentityServer.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Server.Setup;
using Serilog;
using Serilog.Events;
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
    
    public WebApplicationBuilder Builder = WebApplication.CreateBuilder();
    
    public WebApplication? app { get; private set; } 

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        Builder.Configuration["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString();
    }
    
    public Task StartAppAsync(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateBootstrapLogger();

        Log.Information("Starting up");
        
        Builder.AddIdentityServer();
        app = Builder.Build()
                .ConfigurePipeline();
        return app.StartAsync();
    }
    
    public void Configure<T>(Action<T> configure)
    where T : class
    {
        Builder.Services.Configure(configure);
    }

    public async Task DisposeAsync()
    {
        if (app is not null)
        {
            await app.StopAsync();
        }
        
        await _postgresContainer.DisposeAsync();
    }
}
