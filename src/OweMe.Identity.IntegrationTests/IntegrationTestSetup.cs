using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Server.Setup;
using Serilog;
using Testcontainers.PostgreSql;
using Xunit.Sdk;

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

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        Builder.Configuration["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString();
    }
    
    public Task StartAppAsync(ITestOutputHelper testOutputHelper = null!)
    {
        Builder.AddIdentityServer();

        if (testOutputHelper != null)
        {
            Builder.Services.AddLogging(builder => builder.AddXUnit());
        }
        
        app = Builder.Build()
                .ConfigurePipeline();
        return app.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (app is not null)
        {
            await app.StopAsync();
        }
        
        await _postgresContainer.DisposeAsync();
    }
}
