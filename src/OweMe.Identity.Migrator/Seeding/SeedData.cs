namespace OweMe.Identity.Migrator.Seeding;

public sealed record SeedData
{
    public const string SectionName = "Seeding";

    public required List<Scope> Scopes { get; init; }

    public required List<Client> Clients { get; init; }
}
