using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator.Migrations;
using OweMe.Identity.Migrator.Seeding;

namespace OweMe.Identity.Migrator.Orchiestration;

internal static class CommandDelegateHelper
{
    private static readonly Option<bool> VerboseOption = new("--verbose", "-v")
    {
        Description = "Enables verbose logging for the migration process.",
        Recursive = true
    };

    internal static void RegisterCommands(this RootCommand rootCommand, Action<IServiceCollection, IConfiguration>? configureServices = null, Action<IConfigurationBuilder>? configureConfiguration = null)
    {
        rootCommand.BindCommand<MigrateCommand>(new("migrate", "Applies all pending migrations to the database.")
        {
            Options = { VerboseOption }
        }, configureServices, configureConfiguration);

        rootCommand.BindCommand<SeedCommand>(new("seed", "Seeds the database.")
        {
            Options = { VerboseOption }
        }, (services, configuration) =>
        {
            configureServices?.Invoke(services, configuration);
            services.AddSeedCommand(configuration);
        }, configureConfiguration);
    }

    private static void BindCommand<TCommand>(this RootCommand root, Command command, Action<IServiceCollection, IConfiguration>? configureServices = null, Action<IConfigurationBuilder>? configureConfiguration = null)
        where TCommand : class, ICommand
    {
        command.SetAction((parseResult, cancellationToken) =>
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
                configureConfiguration?.Invoke(configurationBuilder);
            var configuration = configurationBuilder.Build();

            bool isVerbose = parseResult.GetValue(VerboseOption);
            var services = DependencyInjection.CreateProvider(configuration, isVerbose);

            services.AddTransient<TCommand>();
            configureServices?.Invoke(services, configuration);

            var serviceProvider = services
                .BuildServiceProvider()
                .ValidateOptions();
            return Run<TCommand>(serviceProvider, cancellationToken);
        });
        root.Add(command);
    }

    private static async Task Run<TCommand>(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        using var scope = serviceProvider.CreateScope();
        var command = scope.ServiceProvider.GetRequiredService<TCommand>();
        try
        {
            await command.ExecuteAsync(cancellationToken);
            Environment.ExitCode = 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the migration process.");
            Environment.ExitCode = 1;
        }
    }
}
