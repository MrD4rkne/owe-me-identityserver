using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.Server.Setup;
using Serilog;
using Testcontainers.PostgreSql;

namespace OweMe.Identity.IntegrationTests.Helpers;

public sealed class AppBuilder : IAsyncDisposable
{
    public required WebApplicationBuilder Builder { get; set; }

    private PostgreSqlContainer? _postgresContainer;

    public AppBuilder Configure<T>(Action<T> configure)
        where T : class
    {
        Builder.Services.Configure(configure);
        return this;
    }

    public AppBuilder WithDatabase()
    {
        if (_postgresContainer != null)
        {
            throw new InvalidOperationException("Database container is already created.");
        }

        _postgresContainer = CreatePostgresSqlContainer();
        return this;
    }

    public AppBuilder WithConnectionString(string connectionString)
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