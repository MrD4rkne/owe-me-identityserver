using Duende.IdentityModel.Client;

namespace OweMe.Identity.IntegrationTests.Helpers;

internal static class TokenHelper
{
    public static async Task<string> GetTokenAsync(HttpClient client, string url, string userName, string password,
        string clientId, string clientSecret, string scope)
    {
        var disco = await client.GetDiscoveryDocumentAsync(url);
        Assert.False(disco.IsError, $"Discovery document is not accessible at {url}: {disco.Error}");

        var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = clientId,
            ClientSecret = clientSecret,
            UserName = userName,
            Password = password,
            Scope = scope
        });

        if (tokenResponse.IsError)
        {
            Assert.Fail("Token request failed: " + tokenResponse.Error);
        }

        if (tokenResponse.AccessToken is null)
        {
            Assert.Fail("Token request failed: AccessToken is null");
        }

        return tokenResponse.AccessToken;
    }

    public static async Task<string> GetTokenAsync(HttpClient client, string url, string clientId, string clientSecret,
        string scope)
    {
        var disco = await client.GetDiscoveryDocumentAsync(url);
        Assert.False(disco.IsError, $"Discovery document is not accessible at {url}: {disco.Error}");

        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scope = scope
        });

        if (tokenResponse.IsError)
        {
            Assert.Fail("Token request failed: " + tokenResponse.Error);
        }

        if (tokenResponse.AccessToken is null)
        {
            Assert.Fail("Token request failed: AccessToken is null");
        }

        return tokenResponse.AccessToken;
    }
}