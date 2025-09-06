using OweMe.Identity.Server.Users.Domain;
using OweMe.Identity.Server.Users.Presentation;

namespace OweMe.Identity.UnitTests.Users.Presentation;

public class GetUserEndpointTests
{
    private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;
    private readonly Guid _nonExistentUserId = Guid.NewGuid();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IUserService> _userServiceMock = new();

    [Fact]
    public async Task ForUnexistentUser_ReturnsNotFound()
    {
        // Arrange
        _userServiceMock.Setup(s => s.GetUserByIdAsync(_nonExistentUserId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await GetUserEndpoint.GetUserById(_nonExistentUserId.ToString(), _userServiceMock.Object,
            _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<NotFound>();

        _userServiceMock.Verify(
            s => s.GetUserByIdAsync(_nonExistentUserId.ToString(),
                It.Is<CancellationToken>(token => token == _cancellationToken)), Times.Once);
        _userServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForExistentUser_ReturnsUser()
    {
        // Arrange
        const string userName = "testuser";
        const string email = "testuser@owe.me";
        var applicationUser = new ApplicationUser
        {
            Id = _userId.ToString(),
            UserName = userName,
            Email = email
        };
        _userServiceMock.Setup(s => s.GetUserByIdAsync(_userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applicationUser);

        // Act
        var result = await GetUserEndpoint.GetUserById(_userId.ToString(), _userServiceMock.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<User>>();
        var okResult = result as Ok<User>;
        okResult.ShouldNotBeNull();
        okResult.Value.Sub.ShouldBe(_userId.ToString());
        okResult.Value.UserName.ShouldBe(userName);
        okResult.Value.Email.ShouldBe(email);

        _userServiceMock.Verify(
            s => s.GetUserByIdAsync(_userId.ToString(), It.Is<CancellationToken>(token => token == _cancellationToken)),
            Times.Once);
    }

    [Theory]
    [InlineData(null, "notnull@owe.me")]
    [InlineData("not null", null)]
    [InlineData(null, null)]
    public async Task ForExistingUser_WithNullUserNameOrEmail_ReturnsNotFound(string? userName, string? email)
    {
        // Arrange
        var applicationUser = new ApplicationUser
        {
            Id = _userId.ToString(),
            UserName = userName!,
            Email = email!
        };
        _userServiceMock.Setup(s => s.GetUserByIdAsync(_userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applicationUser);

        // Act
        var result = await GetUserEndpoint.GetUserById(_userId.ToString(), _userServiceMock.Object, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<NotFound>();

        _userServiceMock.Verify(
            s => s.GetUserByIdAsync(_userId.ToString(), It.Is<CancellationToken>(token => token == _cancellationToken)),
            Times.Once);
        _userServiceMock.VerifyNoOtherCalls();
    }
}