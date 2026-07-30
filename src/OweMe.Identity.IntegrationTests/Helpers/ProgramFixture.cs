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
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5432, true)
        .Build();

    protected ITestOutputHelper? TestOutputHelper { get; private set; }

    public ProgramFixture WithTestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
        return this;
    }

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

        builder.WithConfigure<OperationalStoreOptions>(options => { options.EnableTokenCleanup = false; });

        builder.ConfigureServices(services =>
        {
            services.AddLogging(logging =>
            {
                if (TestOutputHelper != null)
                {
                    logging.AddXUnit(TestOutputHelper);
                }
            });
        });
    }
}
