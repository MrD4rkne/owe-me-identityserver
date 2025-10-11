using Duende.IdentityServer;
using OweMe.Identity.Server.Setup;
using Serilog;
using Serilog.Enrichers.Span;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

if (Log.Logger.GetType().FullName == "Serilog.Core.Pipeline.SilentLogger")
{
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithSpan()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateBootstrapLogger();
}

builder.Host.UseSerilog();
builder.Services.AddSerilog();

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

Log.Information("Starting up");

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
    Log.Fatal(ex, "Unhandled exception during application startup");
}
finally
{
    Log.Information("Shut down complete");
    await Log.CloseAndFlushAsync();
}

public partial class Program;
