using System.CommandLine;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OweMe.Identity.Migrator;
using OweMe.Identity.Migrator.Seeding;
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
            var migrator = BuildMigrator();
            migrator.Parse(["migrate"]).InvokeAsync().GetAwaiter().GetResult();
        }
    }

    private RootCommand BuildMigrator()
    {
        return App.BuildRootCommand((services, _) =>
        {
            services.AddSingleton(Options.Create(_migratorOptions.SeedData!));
            services.AddLogging(logging =>
            {
                if (TestOutputHelper != null)
                {
                    logging.AddXUnit(TestOutputHelper);
                }
            });
        }, configuration =>
        {
            configuration.AddInMemoryCollection([new("ConnectionStrings:DefaultConnection", ConnectionString)]);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        if (_migratorOptions is { ShouldSeed: true, SeedData: not null })
        {
            var migrator = BuildMigrator();
            migrator.Parse(["seed"]).InvokeAsync().GetAwaiter().GetResult();

            SeedUsers(host.Services, _migratorOptions.TestUsers, CancellationToken.None).GetAwaiter().GetResult();
        }

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
