using Duende.IdentityServer;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OweMe.Identity.Persistence;
using OweMe.Identity.Server;
using OweMe.Identity.Server.Data;

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
    .WithLogging(b =>
    {
        b.AddOtlpExporter();
    })
    .WithTracing(b =>
    {
        b.AddAspNetCoreInstrumentation();
        b.AddHttpClientInstrumentation();
        b.AddSource(IdentityServerConstants.Tracing.Basic)
            .AddSource(IdentityServerConstants.Tracing.Cache)
            .AddSource(IdentityServerConstants.Tracing.Services)
            .AddSource(IdentityServerConstants.Tracing.Stores)
            .AddSource(IdentityServerConstants.Tracing.Validation);
        b.AddOtlpExporter();
    })
    .WithMetrics(b =>
    {
        b.AddAspNetCoreInstrumentation();
        b.AddHttpClientInstrumentation();
        b.AddOtlpExporter();
    }).WithLogging();

try
{
    builder.AddConnectionStringFromEnv();

    builder.Services.AddOweMeStorage(builder.Configuration.GetConnectionString(Constants.ConnectionStringName));

    builder.AddIdentityServer();

    var app = builder.Build()
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
