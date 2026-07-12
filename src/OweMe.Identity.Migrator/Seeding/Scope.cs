namespace OweMe.Identity.Migrator.Seeding;

public sealed record Scope
{
    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public string? Description { get; init; }
}
