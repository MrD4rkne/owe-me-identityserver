using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator.Migrations;
using OweMe.Identity.Migrator.Seeding;
using OweMe.Identity.Persistence;
using OweMe.Identity.Persistence.Users.Domain;

namespace OweMe.Identity.IntegrationTests.Helpers;

public sealed class ProgramWithSeedData : ProgramFixture
{
    private readonly MigratorOptions _migratorOptions = new();

    public ProgramWithSeedData()
    {
        WithMigrations();
        WithSeeding(TestSeedData.Data);
        WithTestUser(TestSeedData.TestUser);
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        if (_migratorOptions.ShouldMigrate)
        {
            var services = new ServiceCollection();

            services.AddOweMeStorage(ConnectionString);

            services.AddLogging(logging => logging.AddConsole());

            using var serviceProvider = services.BuildServiceProvider();

            var migrateLogger = serviceProvider.GetRequiredService<ILogger<MigrateCommand>>();
            var migrator = new MigrateCommand(serviceProvider, migrateLogger);
            migrator.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
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

        SeedUsers(host.Services, _migratorOptions.TestUsers, CancellationToken.None).GetAwaiter().GetResult();

        return host;
    }

    private static async Task SeedUsers(IServiceProvider serviceProvider, List<TestUser> testUsers, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var testUser in testUsers)
        {
            var user = new ApplicationUser
            {
                UserName = testUser.Username,
                Email = testUser.Username,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, testUser.Password);
            if (!result.Succeeded)
            {
                throw new SeedingUsersException(result.Errors);
            }
        }
    }

    public sealed class SeedingUsersException(IEnumerable<IdentityError> errors)
        : Exception(string.Join(Environment.NewLine, errors.Select(e => e.Description)));
}
