using Serilog;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Helpers;

public static class IntegrationTestSetup
{
    public static void InitGlobalLogging(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateBootstrapLogger();
    }
}
