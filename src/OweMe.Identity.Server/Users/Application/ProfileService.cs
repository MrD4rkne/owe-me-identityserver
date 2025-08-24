using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using OweMe.Identity.Server.Users.Domain;

namespace OweMe.Identity.Server.Users.Application;

public class ProfileService(UserManager<ApplicationUser> userManager, ILogger<ProfileService> logger) : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await userManager.GetUserAsync(context.Subject);
        if (user is null)
        {
            logger.LogError("User associated with {Subject} not found", context.Subject);
            throw new ArgumentException("User not found");
        }
        
        if(user.UserName is null || user.Email is null)
        {
            logger.LogError("User associated with {Subject} has no username or email", context.Subject);
            throw new ArgumentException("User has no username or email");
        }
        
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, user.Id),
            new(JwtClaimTypes.Name, user.UserName),
            new(JwtClaimTypes.Email, user.Email)
        };
        
        context.IssuedClaims.AddRange(claims);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
       var user = await userManager.GetUserAsync(context.Subject);
       context.IsActive = user is not null;
    }
}