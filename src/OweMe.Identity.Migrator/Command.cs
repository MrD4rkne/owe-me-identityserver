namespace OweMe.Identity.Migrator;

internal interface ICommand
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
