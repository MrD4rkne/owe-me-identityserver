namespace OweMe.Identity.Server.Setup;

public sealed class MigrationsOptions
{
    public const string SectionName = "OweMe:Identity:Migrations";
    
    public bool ApplyMigrations { get; set; } = false;
    
    public bool SeedData { get; set; } = false;
}