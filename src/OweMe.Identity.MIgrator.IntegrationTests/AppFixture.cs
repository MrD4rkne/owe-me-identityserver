using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace OweMe.Identity.Migrator.IntegrationTests;

public sealed class AppFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5432, true)
        .Build();

    public string ConnectionString => _databaseContainer.GetConnectionString();

    public RootCommand Build(ITestOutputHelper testOutputHelper)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _databaseContainer.GetConnectionString());
        return App.BuildRootCommand((services, _) => { services.AddLogging(logging => logging.AddXUnit(testOutputHelper)); });
    }

    public Task InitializeAsync()
    {
        return _databaseContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _databaseContainer.DisposeAsync().AsTask();
    }
}
