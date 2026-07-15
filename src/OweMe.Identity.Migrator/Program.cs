using System.CommandLine;
using System.Diagnostics;
using OweMe.Identity.Migrator;
using OweMe.Identity.Migrator.Orchiestration;

RootCommand rootCommand = new()
{
    Description =
        $"{Process.GetCurrentProcess().ProcessName} - A tool for managing database migrations for the OweMe.Identity project.",
};
rootCommand.RegisterCommands();

return await rootCommand.Parse(args).InvokeAsync();
