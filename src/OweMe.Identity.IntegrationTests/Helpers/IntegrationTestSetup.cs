using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Helpers;

public static class IntegrationTestSetup
{
    public static AppBuilder Create()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("https://[::1]:0");

        var app = new AppBuilder
        {
            Builder = builder
        };
        return app;
    }

    public static void InitGlobalLogging(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateBootstrapLogger();
    }
}
