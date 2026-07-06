namespace OweMe.Identity.Persistence.Users.Domain;

public interface IUserService
{
    Task<ApplicationUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
}
