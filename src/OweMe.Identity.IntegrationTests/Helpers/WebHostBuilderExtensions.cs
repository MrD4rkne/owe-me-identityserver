using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OweMe.Identity.IntegrationTests.Helpers;

internal static class WebHostBuilderExtensions
{
    public static IWebHostBuilder WithConfigures<T>(this IWebHostBuilder builder, Action<T> configure)
    where T : class
    {
        return builder.ConfigureServices(services =>
        {
            services.Configure(configure);
        });
    }

    public static IWebHostBuilder WithConnectionString(this IWebHostBuilder builder, string connectionString)
    {
        return builder.ConfigureAppConfiguration((_, config) =>
                       {
                           config.AddInMemoryCollection(new Dictionary<string, string?>
                           {
                               ["ConnectionStrings:DefaultConnection"] = connectionString
                           });
                       });
    }

    public static IWebHostBuilder WithSetting(this IWebHostBuilder builder, string key, string value)
    {
        
    }
}
