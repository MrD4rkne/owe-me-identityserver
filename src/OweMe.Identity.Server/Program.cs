using OweMe.Identity.Server.Setup;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    builder.Host.UseSerilog();
    builder.Services.AddSerilog();

    var app = builder
        .AddIdentityServer()
        .Build()
        .ConfigurePipeline();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design") // see https://github.com/dotnet/efcore/issues/29923
{
    Log.Fatal(ex, "Unhandled exception during application startup");
}
finally
{
    Log.Information("Shut down complete");
    await Log.CloseAndFlushAsync();
}