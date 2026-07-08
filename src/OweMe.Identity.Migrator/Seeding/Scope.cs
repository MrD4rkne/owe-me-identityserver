namespace OweMe.Identity.Migrator.Seeding;

internal readonly record struct Scope
{
    public string Name { get; init; }

    public string DisplayName { get; init; }

    public string Description { get; init; }
}
