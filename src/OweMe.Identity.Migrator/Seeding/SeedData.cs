using Microsoft.Extensions.Options;

namespace OweMe.Identity.Migrator.Seeding;

public sealed record SeedData
{
    public const string SectionName = "Seeding";

    [ValidateEnumeratedItems]
    public required List<Scope> Scopes { get; init; } = [];

    [ValidateEnumeratedItems]

    public required List<Client> Clients { get; init; } = [];
}
