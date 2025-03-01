using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;

namespace OweMe.Identity.Server.Users;

public class ProfileService(UserManager<ApplicationUser> userManager, ILogger<ProfileService> profileService) : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await userManager.GetUserAsync(context.Subject);
        if (user is null)
        {
            profileService.LogError("User associated with {subject} not found", context.Subject);
            throw new ArgumentException("User not found");
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