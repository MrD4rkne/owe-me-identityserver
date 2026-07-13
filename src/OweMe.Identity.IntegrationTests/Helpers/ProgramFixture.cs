using Duende.IdentityServer.EntityFramework.Options;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator.Migrations;
using OweMe.Identity.Migrator.Seeding;
using OweMe.Identity.Persistence;
using OweMe.Identity.Persistence.Users.Domain;
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

    private readonly MigratorOptions _migratorOptions = new();

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

        if (_migratorOptions.ShouldMigrate)
        {
            var services = new ServiceCollection();

            services.AddOweMeStorage(connectionString);

            services.AddLogging(logging => logging.AddConsole());

            using var serviceProvider = services.BuildServiceProvider();

            var migrateLogger = serviceProvider.GetRequiredService<ILogger<MigrateCommand>>();
            var migrator = new MigrateCommand(serviceProvider, migrateLogger);
            migrator.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        foreach (var configureTestService in _configureTestServices)
        {
            configureTestService(builder);
        }

        builder.WithConfigure<OperationalStoreOptions>(options => { options.EnableTokenCleanup = false; });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        if (_migratorOptions is { ShouldSeed: true, SeedData: not null })
        {
            var seedLogger = host.Services.GetRequiredService<ILogger<SeedCommand>>();
            using var scope = host.Services.CreateAsyncScope();
            var seedCommand = new SeedCommand(
                scope.ServiceProvider,
                seedLogger,
                Microsoft.Extensions.Options.Options.Create(_migratorOptions.SeedData)
            );
            seedCommand.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        using var userscope = host.Services.CreateAsyncScope();
        var userManager = userscope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var testUser in _migratorOptions.TestUsers)
        {
            var user = new ApplicationUser
            {
                UserName = testUser.Username,
                Email = testUser.Username,
                EmailConfirmed = true,
            };

            var result = userManager.CreateAsync(user, testUser.Password).GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
            }
        }

        return host;
    }

    public ProgramFixture ConfigureTestServices(Action<IWebHostBuilder> configure)
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

    public ProgramFixture WithMigrations()
    {
        _migratorOptions.ShouldMigrate = true;
        return this;
    }

    public ProgramFixture WithSeeding(SeedData seedData)
    {
        _migratorOptions.ShouldSeed = true;
        _migratorOptions.SeedData = seedData;
        return this;
    }

    public ProgramFixture WithTestUser(TestUser testUser)
    {
        _migratorOptions.TestUsers.Add(testUser);
        return this;
    }

    private sealed record MigratorOptions
    {
        public bool ShouldMigrate { get; set; } = false;
        public bool ShouldSeed { get; set; } = false;
        public SeedData? SeedData { get; set; } = null;
        public List<TestUser> TestUsers { get; } = [];
    }
}
