using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace OweMe.Identity.Server.Setup;

public sealed class IdentityConfig
{
    public const string SectionName = "OweMe:Identity";

    public readonly IEnumerable<IdentityResource> IdentityResources =
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new(name:"user", userClaims: [JwtClaimTypes.Email])
    ];

    public ApiScope[] ApiScopes { get; set; } = [];

    public Client[] Clients { get; set; } = [];
    
    public TestUser[] Users { get; set; } = [];
}