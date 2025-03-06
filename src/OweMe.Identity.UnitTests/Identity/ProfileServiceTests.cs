using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using OweMe.Identity.Server.Users;
using Xunit;

namespace OweMe.Identity.UnitTests;

public class ProfileServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<ProfileService>> _loggerMock;
    private readonly ProfileService _profileService;

    public ProfileServiceTests()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager(new List<ApplicationUser>());
        _loggerMock = new Mock<ILogger<ProfileService>>();
        _profileService = new ProfileService(_userManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProfileDataAsync_UserExists_AddsClaims()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1", UserName = "testuser", Email = "test@example.com" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        var context = new ProfileDataRequestContext(new ClaimsPrincipal(), new Client(), "test", []);

        // Act
        await _profileService.GetProfileDataAsync(context);

        // Assert
        Assert.Contains(context.IssuedClaims, claim => claim.Type == JwtClaimTypes.Subject && claim.Value == user.Id);
        Assert.Contains(context.IssuedClaims, claim => claim.Type == JwtClaimTypes.Name && claim.Value == user.UserName);
        Assert.Contains(context.IssuedClaims, claim => claim.Type == JwtClaimTypes.Email && claim.Value == user.Email);
    }
    
    [Fact]
    public async Task GetProfileDataAsync_UsernameNull_ThrowsArgumentException()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1", UserName = null, Email = "test@example.com" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        var context = new ProfileDataRequestContext(new ClaimsPrincipal(), new Client(), "test", []);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _profileService.GetProfileDataAsync(context));
    }
    
    [Fact]
    public async Task GetProfileDataAsync_EmailNull_ThrowsArgumentException()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1", UserName = "test", Email = null };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        var context = new ProfileDataRequestContext(new ClaimsPrincipal(), new Client(), "test", []);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _profileService.GetProfileDataAsync(context));
    }

    [Fact]
    public async Task GetProfileDataAsync_UserNotFound_ThrowsArgumentException()
    {
        // Arrange
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);
        var context = new ProfileDataRequestContext(new ClaimsPrincipal(), new Client(), "test", []);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _profileService.GetProfileDataAsync(context));
    }

    [Fact]
    public async Task IsActiveAsync_UserExists_SetsIsActiveToTrue()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        var context = new IsActiveContext(new ClaimsPrincipal(), new Client(), "test");

        // Act
        await _profileService.IsActiveAsync(context);

        // Assert
        Assert.True(context.IsActive);
    }

    [Fact]
    public async Task IsActiveAsync_UserNotFound_SetsIsActiveToFalse()
    {
        // Arrange
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);
        var context = new IsActiveContext(new ClaimsPrincipal(), new Client(), "test");

        // Act
        await _profileService.IsActiveAsync(context);

        // Assert
        Assert.False(context.IsActive);
    }
}