using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace OweMe.Identity.IntegrationTests.Helpers;

internal static class WebHostBuilderExtensions
{
    public static IWebHostBuilder WithConfigure<T>(this IWebHostBuilder builder, Action<T> configure)
    where T : class
    {
        return builder.ConfigureServices(services =>
        {
            services.Configure(configure);
        });
    }
}
