using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace OweMe.Identity.Server.Setup;

public class Config(IConfiguration configuration)
{
    private const string TEST_USERS_SECTION = "OweMe:Identity:TestUsers";
    private const string CLIENTS_SECTION = "OweMe:Identity:Clients";
    private const string API_SCOPES_SECTION = "OweMe:Identity:ApiScopes";

    public readonly IEnumerable<IdentityResource> IdentityResources =
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new(name:"user", userClaims: [JwtClaimTypes.Email])
    ];

    public readonly ApiScope[] ApiScopes = configuration.GetSection(API_SCOPES_SECTION).Get<ApiScope[]>() ?? [];

    public readonly Client[] Clients = configuration.GetSection(CLIENTS_SECTION).Get<Client[]>() ?? [];
    
    public readonly TestUser[] Users = configuration.GetSection(TEST_USERS_SECTION).Get<TestUser[]>() ?? [];
}