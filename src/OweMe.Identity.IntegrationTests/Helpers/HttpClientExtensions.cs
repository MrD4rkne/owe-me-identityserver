namespace OweMe.Identity.IntegrationTests.Helpers;

public class UnsecureHttpClientFactory
{
    public HttpClient CreateUnsecureClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        return new HttpClient(handler);
    }
}