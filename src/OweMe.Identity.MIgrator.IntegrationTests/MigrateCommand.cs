using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Migrator.Migrations.Factories;
using OweMe.Identity.Persistence.IdentityServer;
using OweMe.Identity.Persistence.Users;
using Shouldly;
using Xunit.Abstractions;

namespace OweMe.Identity.Migrator.IntegrationTests;

public sealed class MigrateCommand(AppFixture fixture, ITestOutputHelper testOutputHelper) : IClassFixture<AppFixture>
{
    [Fact]
    public async Task Should_RunMigrations()
    {
        // Arrange
        var sut = fixture.Build(testOutputHelper);

        // Act
        var result = await sut.Parse("migrate").InvokeAsync();

        // Assert
        Assert.Equal(0, result);
        AssertMigrated<ApplicationDbContextFactory, ApplicationDbContext>(fixture.ConnectionString);
        AssertMigrated<ConfigurationDbContextFactory, ConfigurationDbContext>(fixture.ConnectionString);
        AssertMigrated<PersistedGrantDbContextFactory, PersistedGrantDbContext>(fixture.ConnectionString);
        AssertMigrated<DataProtectionDbContextFactory, DataProtectionDbContext>(fixture.ConnectionString);
    }

    private static void AssertMigrated<TContextFactory, TContext>(string connectionString) where TContextFactory : BaseDbContextFactory<TContext> where TContext : DbContext
    {
        var factory = Activator.CreateInstance<TContextFactory>();
        var context = factory.CreateDbContext(connectionString);
        var pendingMigrations = context.Database.GetPendingMigrations();
        pendingMigrations.ShouldBeEmpty($"There are pending migrations for context {typeof(TContext).Name}");
    }
}
