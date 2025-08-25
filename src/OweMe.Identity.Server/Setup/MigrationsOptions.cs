namespace OweMe.Identity.Server.Setup;

public sealed record MigrationsOptions
{
    public const string SectionName = "OweMe:Identity:Migrations";
    
    public bool ApplyMigrations { get; init; } = true;
    
    public bool SeedData { get; init; } = true;
}