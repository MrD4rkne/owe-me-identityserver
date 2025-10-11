using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Helpers;

public abstract class TestWithLoggingBase
{
    protected readonly ITestOutputHelper OutputHelper;

    protected TestWithLoggingBase(ITestOutputHelper helper)
    {
        IntegrationTestSetup.InitGlobalLogging(helper);
        OutputHelper = helper;
    }
}
