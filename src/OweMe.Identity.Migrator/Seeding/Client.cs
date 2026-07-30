using System.ComponentModel.DataAnnotations;
using Duende.IdentityServer;
using Microsoft.Extensions.Options;

namespace OweMe.Identity.Migrator.Seeding;

public sealed record Client
{
    [Required]
    public required string ClientId { get; init; }

    [ValidateEnumeratedItems]
    public required Secret[] ClientSecrets { get; init; }

    [Required]
    public required string ClientName { get; init; }

    public string? Description { get; init; }

    public string[] AllowedGrantTypes { get; init; } = [];

    public string[] AllowedScopes { get; init; } = [];

    public sealed record Secret
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
}
