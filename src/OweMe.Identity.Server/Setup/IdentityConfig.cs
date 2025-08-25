using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace OweMe.Identity.Server.Setup;

public sealed record IdentityConfig
{
    public const string SectionName = "OweMe:Identity";

    public readonly IEnumerable<IdentityResource> IdentityResources =
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new(name:"user", userClaims: [JwtClaimTypes.Email])
    ];

    public required ApiScope[] ApiScopes { get; init; }

    public required Client[] Clients { get; init; }
    
    public required TestUser[] Users { get; init; }
}