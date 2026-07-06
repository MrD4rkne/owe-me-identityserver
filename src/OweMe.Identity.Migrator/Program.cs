using System.CommandLine;
using System.Diagnostics;
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
    Arguments = { connectionStringArgument }
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

    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddSimpleConsole(options =>
            {
                options.IncludeScopes = false;
                options.TimestampFormat = "[HH:mm:ss] ";
                options.SingleLine = true;
            });
        if (isVerbose)
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
        }
        else
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        }
    });

    var logger = loggerFactory.CreateLogger("MigrationPipeline");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        logger.LogError("Database connection string was not provided. Provide it as an argument or set the '{Variable}' environment variable.", connectionStringVariableName);
        Environment.ExitCode = 1;
        return;
    }

    try
    {
        await new MigrateCommand(loggerFactory).ExecuteAsync(connectionString, cancellationToken);
        Environment.ExitCode = 0;
        return;
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "An error occurred during the migration process.");
        Environment.ExitCode = 1;
        return;
    }
});

return await rootCommand.Parse(args).InvokeAsync();
