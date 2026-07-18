using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Server.Data;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Helpers;

public class ProgramFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly List<Action<IWebHostBuilder>> _configureTestServices = new();

    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5432, true)
        .Build();

    protected string ConnectionString => _databaseContainer.GetConnectionString();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        return _databaseContainer.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _databaseContainer.StartAsync().GetAwaiter().GetResult();
        var connectionString = _databaseContainer.GetConnectionString();

        builder.UseSetting($"ConnectionStrings:{Constants.ConnectionStringName}", connectionString);

        foreach (var configureTestService in _configureTestServices)
        {
            configureTestService(builder);
        }

        builder.WithConfigure<OperationalStoreOptions>(options => { options.EnableTokenCleanup = false; });
    }

    private ProgramFixture ConfigureTestServices(Action<IWebHostBuilder> configure)
    {
        _configureTestServices.Add(configure);
        return this;
    }

    public ProgramFixture AddLogging(ITestOutputHelper testOutputHelper)
    {
        return ConfigureTestServices(configure => configure.ConfigureServices(services =>
        {
            services.AddLogging((builder) => builder.AddXUnit(testOutputHelper));
        }));
    }
}
