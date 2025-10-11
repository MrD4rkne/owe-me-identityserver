using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Serilog;
using Testcontainers.PostgreSql;

namespace OweMe.Identity.IntegrationTests;

public sealed class ProgramFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services => services.AddSerilog());

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var connectionString = _databaseContainer.GetConnectionString();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            });
        });
    }

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
}
