using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace OweMe.Identity.Server.Setup;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new("roles", "User roles", [JwtClaimTypes.Role])
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new("oweme-api", "Owe-Me API")
    ];

    public static IEnumerable<Client> Clients =>
    [
        new()
        {
                ClientId = "oweme-client",
                ClientSecrets = { 
                    new Secret("oweme-secret".Sha256())
                },
                AllowedScopes = ["oweme-api"],
                AllowedGrantTypes = [OidcConstants.GrantTypes.Password, OidcConstants.GrantTypes.ClientCredentials],
                AlwaysSendClientClaims = true,
                AllowAccessTokensViaBrowser = true
            }
    ];
    
    public static List<TestUser> Users =>
    [
        new()
        {
            SubjectId = "1",
            Username = "alice",
            Password = "password"
        }
    ];
}