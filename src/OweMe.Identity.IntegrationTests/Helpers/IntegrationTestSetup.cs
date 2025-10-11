using Serilog;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Helpers;

public static class IntegrationTestSetup
{
    public static void InitGlobalLogging(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(testOutputHelper, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
