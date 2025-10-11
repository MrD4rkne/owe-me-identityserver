using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OweMe.Identity.IntegrationTests.Helpers;
using Testcontainers.PostgreSql;

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
}
