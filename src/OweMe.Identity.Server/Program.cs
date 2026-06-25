using Duende.IdentityServer;
using OweMe.Identity.Server.Setup;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Startup");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .WithLogging()
    .WithTracing(b =>
    {
        b.AddAspNetCoreInstrumentation();
        b.AddHttpClientInstrumentation();
        b.AddSource(IdentityServerConstants.Tracing.Basic)
            .AddSource(IdentityServerConstants.Tracing.Cache)
            .AddSource(IdentityServerConstants.Tracing.Services)
            .AddSource(IdentityServerConstants.Tracing.Stores)
            .AddSource(IdentityServerConstants.Tracing.Validation);
    })
    .WithMetrics(b =>
    {
        b.AddAspNetCoreInstrumentation();
        b.AddHttpClientInstrumentation();
    }).WithLogging();

try
{
    var app = builder
        .AddIdentityServer()
        .Build()
        .ConfigurePipeline();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design") // see https://github.com/dotnet/efcore/issues/29923
{
    bootstrapLogger.LogCritical(ex, "Unhandled exception during application startup");
}
finally
{
    bootstrapLogger.LogInformation("Shut down complete");
}

public partial class Program;
