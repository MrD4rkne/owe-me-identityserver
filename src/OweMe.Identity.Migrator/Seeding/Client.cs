using System.ComponentModel.DataAnnotations;
using Duende.IdentityServer;

namespace OweMe.Identity.Migrator.Seeding;

internal sealed record Client
{
    public required string ClientId { get; init; }

    public required Secret[] ClientSecrets { get; init; }

    public required string ClientName { get; init; }

    public string? Description { get; init; }

    public string[] AllowedGrantTypes { get; init; } = [];

    public string[] AllowedScopes { get; init; } = [];

    internal sealed record Secret
    {
        public required string Value { get; init; }

        [AllowedValues([
            IdentityServerConstants.SecretTypes.SharedSecret, IdentityServerConstants.SecretTypes.JsonWebKey,
            IdentityServerConstants.SecretTypes.X509CertificateBase64, IdentityServerConstants.SecretTypes.JsonWebKey,
            IdentityServerConstants.SecretTypes.X509CertificateThumbprint
        ])]
        public string Type { get; init; } = IdentityServerConstants.SecretTypes.SharedSecret;

        public DateTimeOffset? Expiration { get; init; }

        public string? Description { get; init; }
    }

    internal sealed record GrantType
    {
        [AllowedValues([
            "client-credentials",
            "password"
        ])]
        public required string Value { get; init; }
    }
}
