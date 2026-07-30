using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OweMe.Identity.Persistence.IdentityServer;

public sealed class DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}
