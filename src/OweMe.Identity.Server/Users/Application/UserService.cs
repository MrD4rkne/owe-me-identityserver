using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Persistence.Users;
using OweMe.Identity.Persistence.Users.Domain;

namespace OweMe.Identity.Server.Users.Application;

public class UserService(ApplicationDbContext dbContext) : IUserService
{
    public Task<ApplicationUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }
}
