using Serilog;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Helpers;

public abstract class TestWithLoggingBase
{
    protected readonly ITestOutputHelper OutputHelper;

    protected TestWithLoggingBase(ITestOutputHelper helper)
    {
        OutputHelper = helper;
        InitGlobalLogging(helper);
    }

    private static void InitGlobalLogging(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(testOutputHelper, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
