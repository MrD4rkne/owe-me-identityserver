namespace OweMe.Identity.Server.Setup;

public sealed class MigrationsOptions
{
    public const string SectionName = "Migrations";
    
    public bool ApplyMigrations { get; set; } = false;
    
    public bool SeedData { get; set; } = false;
}