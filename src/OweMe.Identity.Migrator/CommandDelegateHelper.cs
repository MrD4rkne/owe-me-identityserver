using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator.Migrations;
using OweMe.Identity.Migrator.Seeding;

namespace OweMe.Identity.Migrator;

internal static class CommandDelegateHelper
{
    private static readonly Argument<string?> ConnectionStringArgument = new("connectionString")
    {
        Description = $"The connection string to the database.",
        Arity = ArgumentArity.ZeroOrOne
    };

    private static readonly Option<bool> VerboseOption = new("--verbose", "-v")
    {
        Description = "Enables verbose logging for the migration process.",
        Recursive = true
    };

    internal static void RegisterCommands(this RootCommand rootCommand)
    {
        rootCommand.BindCommand<MigrateCommand>(new("migrate", "Applies all pending migrations to the database.")
        {
            Arguments = { ConnectionStringArgument },
            Options = { VerboseOption }
        });

        rootCommand.BindCommand<SeedCommand>(new("seed", "Seeds the database.")
        {
            Arguments = { ConnectionStringArgument },
            Options = { VerboseOption }
        });
    }

    internal static void BindCommand<TCommand>(this RootCommand root, Command command)
        where TCommand : ICommand
    {
        command.SetAction(Run<TCommand>);
        root.Add(command);
    }

    private static async Task Run<TCommand>(ParseResult parseResult, CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddConnectionStringFromCli(parseResult)
            .Build();

        bool isVerbose = parseResult.GetValue(VerboseOption);
        var serviceProvider = DependencyInjection.CreateProvider(configuration, isVerbose);
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            using var scope = serviceProvider.CreateScope();
            var command = scope.ServiceProvider.GetRequiredService<TCommand>();
            await command.ExecuteAsync(cancellationToken);
            Environment.ExitCode = 0;
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the migration process.");
            Environment.ExitCode = 1;
            return;
        }
    }

    private static IConfigurationBuilder AddConnectionStringFromCli(this IConfigurationBuilder builder,
        ParseResult parseResult)
    {
        var connectionString = parseResult.GetValue(ConnectionStringArgument);
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.AddInMemoryCollection([new("ConnectionStrings::DefaultConnection", connectionString)]);
        }

        return builder;
    }
}
