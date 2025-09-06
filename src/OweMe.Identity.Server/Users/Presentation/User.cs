namespace OweMe.Identity.Server.Users.Presentation;

public sealed record User
{
    public required string Sub { get; init; }

    public required string Email { get; init; }

    public string? UserName { get; init; }
}