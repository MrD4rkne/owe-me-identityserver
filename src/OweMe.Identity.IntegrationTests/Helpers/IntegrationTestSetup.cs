using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.Json;
using Serilog;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Helpers;

public static class IntegrationTestSetup
{
    public static AppBuilder Create()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("https://[::1]:0");
        builder.RemoveJsonConfigs();

        var app = new AppBuilder
        {
            Builder = builder
        };
        
        return app;
    }

    private static void RemoveJsonConfigs(this WebApplicationBuilder builder)
    {
        var jsonConfigs = builder.Configuration.Sources.OfType<JsonConfigurationSource>().ToList();
        foreach (var jsonConfig in jsonConfigs)
        {
            builder.Configuration.Sources.Remove(jsonConfig);
        }
    }

    public static void InitGlobalLogging(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateBootstrapLogger();
    }
}
