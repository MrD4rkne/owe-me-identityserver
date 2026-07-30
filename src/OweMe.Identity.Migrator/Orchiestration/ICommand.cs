namespace OweMe.Identity.Migrator.Orchiestration;

internal interface ICommand
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
