using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator;

RootCommand rootCommand = new()
{
    Description = $"{Process.GetCurrentProcess().ProcessName} - A tool for managing database migrations for the OweMe.Identity project.",
};

const string connectionStringVariableName = "MIGRATIONS_CONNECTION_STRING";
Argument<string?> connectionStringArgument = new("connectionString")
{
    Description = $"The connection string to the database. If not provided, the '{connectionStringVariableName}' environment variable will be used.",
    Arity = ArgumentArity.ZeroOrOne
};
Command migrateCommand = new("migrate", "Applies all pending migrations to the database.")
{
    Arguments = { connectionStringArgument },
};
rootCommand.Add(migrateCommand);

Option<bool> verboseOption = new("--verbose", "-v")
{
    Description = "Enables verbose logging for the migration process.",
    Recursive = true
};
migrateCommand.Add(verboseOption);

migrateCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionStringArgument) ?? Environment.GetEnvironmentVariable(connectionStringVariableName);
    var isVerbose = parseResult.GetValue(verboseOption);

    var serviceProvider = DependencyInjection.CreateProvider(connectionString, isVerbose);
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        logger.LogError("Database connection string was not provided. Provide it as an argument or set the '{Variable}' environment variable.", connectionStringVariableName);
        Environment.ExitCode = 1;
        return;
    }

    try
    {
        using var scope = serviceProvider.CreateScope();
        var command = scope.ServiceProvider.GetRequiredService<MigrateCommand>();
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
});

return await rootCommand.Parse(args).InvokeAsync();
