namespace OweMe.Identity.Migrator.Seeding;

internal sealed record SeedData
{
    public const string SectionName = "Seeding";

    public required List<Scope> Scopes { get; init; }
}
