using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using OweMe.Identity.IntegrationTests.Helpers;
using Testcontainers.PostgreSql;

namespace OweMe.Identity.IntegrationTests;

public sealed class ProgramFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
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
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            string? connectionString = _databaseContainer.GetConnectionString();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            });
        });

        builder.WithConfigure<OperationalStoreOptions>(options =>
        {
            options.EnableTokenCleanup = false; // Disable token cleanup during tests
        });
    }
}
