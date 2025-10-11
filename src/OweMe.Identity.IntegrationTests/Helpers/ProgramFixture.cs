using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OweMe.Identity.IntegrationTests.Helpers;
using Serilog;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests;

public sealed class ProgramFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly List<Action<IWebHostBuilder>> _configureTestServices = new();

    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5432, true)
        .Build();

    public Task InitializeAsync()
    {
        return _databaseContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return _databaseContainer.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.WithConnectionString(_databaseContainer.GetConnectionString());

        foreach (var configureTestService in _configureTestServices)
        {
            configureTestService(builder);
        }

        builder.WithConfigure<OperationalStoreOptions>(options => { options.EnableTokenCleanup = false; });
    }

    /// <summary>
    ///     Configure test services for the application.
    /// </summary>
    /// <param name="configure">Action to configure the web host builder.</param>
    public ProgramFixture ConfigureTestServices(Action<IWebHostBuilder> configure)
    {
        _configureTestServices.Add(configure);
        return this;
    }

    public ProgramFixture AddLogging(ITestOutputHelper testOutputHelper)
    {
        return ConfigureTestServices(configure => configure.ConfigureServices(services =>
        {
            services.AddSerilog(
                new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.TestOutput(testOutputHelper,
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger());
        }));
    }
}
