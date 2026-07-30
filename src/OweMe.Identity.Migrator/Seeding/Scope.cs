using System.ComponentModel.DataAnnotations;

namespace OweMe.Identity.Migrator.Seeding;

public sealed record Scope
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required string DisplayName { get; init; }

    public string? Description { get; init; }
}
