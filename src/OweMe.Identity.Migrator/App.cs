using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.Migrator.Orchiestration;

namespace OweMe.Identity.Migrator;

public static class App
{
    public static RootCommand BuildRootCommand(Action<IServiceCollection, IConfiguration>? configureServices = null, Action<IConfigurationBuilder>? configureConfiguration = null)
    {
        RootCommand rootCommand = new()
        {
            Description =
                $"{Process.GetCurrentProcess().ProcessName} - A tool for managing database migrations for the OweMe.Identity project.",
        };
        rootCommand.RegisterCommands(configureServices, configureConfiguration);
        return rootCommand;
    }
}
