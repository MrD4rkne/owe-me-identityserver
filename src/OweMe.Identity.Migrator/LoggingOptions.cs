namespace OweMe.Identity.Migrator;

public sealed record LoggingOptions
{
    public const string SectionName = "Logging";

    public bool Verbose { get; init; }
}
