using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace OweMe.Identity.Server.Setup;

public class Config(IConfiguration configuration)
{
    private const string OWEME_CLIENT_ID = "oweme-client";
    private const string OWEME_CLIENT_SECRET_SECTION = "OweMe:Api:ClientSecret";
    private static readonly string[] OWEME_CLIENT_SCOPES = ["oweme-api"];
    
    private const string OWEME_WEB_CLIENT_ID = "oweme-web";
    private const string OWEME_WEB_CLIENT_SECRET_SECTION = "OweMe:Web:ClientSecret";
    private const string OWEME_WEB_CLIENT_REDIRECT_URIS_SECTION = "OweMe::Web:RedirectUris";
    private const string OWEME_WEB_CLIENT_POST_LOGOUT_REDIRECT_URIS_SECTION = "OweMe:Web:CallbackUris";
    
    private const string TEST_USERS_SECTION = "OweMe:TestUsers";

    public readonly IEnumerable<IdentityResource> IdentityResources =
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new(name:"user", userClaims: [JwtClaimTypes.Email])
    ];

    public readonly IEnumerable<ApiScope> ApiScopes =
    [
        new("oweme-api", "Owe-Me API")
    ];

    public readonly IEnumerable<Client> Clients =
    [
        new()
        {
            ClientId = OWEME_CLIENT_ID,
            ClientSecrets = { 
                new Secret(configuration[OWEME_CLIENT_SECRET_SECTION].Sha256())
            },
            AllowedScopes = OWEME_CLIENT_SCOPES,
            AllowedGrantTypes = [OidcConstants.GrantTypes.Password, OidcConstants.GrantTypes.ClientCredentials],
            AlwaysSendClientClaims = true,
            AllowAccessTokensViaBrowser = true
        },
        new()
        {
            ClientId = OWEME_WEB_CLIENT_ID,
            ClientSecrets = { new Secret(configuration[OWEME_WEB_CLIENT_SECRET_SECTION].Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = configuration.GetSection(OWEME_WEB_CLIENT_REDIRECT_URIS_SECTION).Get<string[]>() ?? [], 
            PostLogoutRedirectUris = configuration.GetSection(OWEME_WEB_CLIENT_POST_LOGOUT_REDIRECT_URIS_SECTION).Get<string[]>() ?? [],
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile
            }
        }
    ];
    
    public readonly TestUser[] Users = configuration.GetSection(TEST_USERS_SECTION).Get<TestUser[]>() ?? [];
}