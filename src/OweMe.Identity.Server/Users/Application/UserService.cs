using Microsoft.EntityFrameworkCore;
using OweMe.Identity.Server.Users.Domain;
using OweMe.Identity.Server.Users.Persistence;

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