using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace OweMe.Identity.IntegrationTests.Helpers;

internal static class WebApplicationFactoryExtensions
{
    /// <summary>
    ///     Ensures that the IdentityServer is initialized by making a request to the discovery endpoint.
    /// </summary>
    public static async Task EnsureInitialized<T>(this WebApplicationFactory<T> factory)
        where T : class
    {
        using var client = factory.CreateClient();

        var response = await client.GetDiscoveryDocumentAsync();
        response.IsError.ShouldBeFalse();
        response.Error.ShouldBeNullOrEmpty();
    }
}
