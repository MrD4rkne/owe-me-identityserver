namespace OweMe.Identity.IntegrationTests.Helpers;

public static class UnsecureHttpClientFactory
{
    public static HttpClient CreateUnsecureClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        return new HttpClient(handler);
    }
}