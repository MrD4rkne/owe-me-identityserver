using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Migrator;
using OweMe.Identity.Migrator.Migrations;

RootCommand rootCommand = new()
{
    Description =
        $"{Process.GetCurrentProcess().ProcessName} - A tool for managing database migrations for the OweMe.Identity project.",
};
rootCommand.RegisterCommands();

return await rootCommand.Parse(args).InvokeAsync();
