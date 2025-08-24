using Microsoft.AspNetCore.Builder;
using OweMe.Identity.Server.Setup;
using Testcontainers.PostgreSql;

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
    
    public Task StartAppAsync()
    {
        app = Builder
            .AddIdentityServer()
            .Build().ConfigurePipeline();
        return app.StartAsync();
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
