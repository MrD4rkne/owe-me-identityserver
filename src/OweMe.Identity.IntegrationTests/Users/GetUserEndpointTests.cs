using System.Net;
using Duende.IdentityModel.Client;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using OweMe.Identity.IntegrationTests.Helpers;
using Shouldly;
using Xunit.Abstractions;

namespace OweMe.Identity.IntegrationTests.Users;

public sealed class GetUserEndpointTests(ITestOutputHelper testOutputHelper, ProgramWithSeedData factory)
    : IClassFixture<ProgramWithSeedData>
{
    private readonly WebApplicationFactory<Program> _factory = factory.WithTestOutputHelper(testOutputHelper);
    private readonly Guid _nonExistentUserId = Guid.NewGuid();

    [Fact]
    public async Task For_ClientWithoutProperScope_Should_ReturnUnauthorized()
    {
        // Arrange
        var client = await _factory.CreateClient()
            .WithToken(TestSeedData.TestUser.Username, TestSeedData.TestUser.Password, TestSeedData.SomeApiClient.ClientId, TestSeedData.SomeApiClientSecret, TestSeedData.SomeScope.Name);
        var existingUserId = await GetUser(_factory, TestSeedData.TestUser.Username, TestSeedData.TestUser.Password);

        // Act
        var response = await client.GetAsync($"/users/{existingUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task For_ClientWithProperScope_ShouldReturnResult()
    {
        // Arrange
        var client = await _factory.CreateClient()
            .WithToken(TestSeedData.LocalApiClient.ClientId, TestSeedData.LocalApiClientSecret, IdentityServerConstants.LocalApi.ScopeName);
        var existingUserId = await GetUser(_factory, TestSeedData.TestUser.Username, TestSeedData.TestUser.Password);

        // Act
        var response = await client.GetAsync($"/users/{existingUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();

        var obj = JObject.Parse(content);
        obj.ShouldNotBeNull();
        obj["sub"]?.Value<string>()?.ShouldBe(existingUserId.ToString());
        obj["email"]?.Value<string>()?.ShouldBe(TestSeedData.TestUser.Username);
        obj["userName"]?.Value<string>()?.ShouldBe(TestSeedData.TestUser.Username);
        obj.Properties().Count().ShouldBe(3, "Response should only contain sub, email and userName");
    }

    [Fact]
    public async Task For_NonExistingUser_Should_ReturnNotFound()
    {
        // Arrange
        var client = await _factory.CreateClient()
            .WithToken(TestSeedData.LocalApiClient.ClientId, TestSeedData.LocalApiClientSecret, IdentityServerConstants.LocalApi.ScopeName);

        // Act
        var response = await client.GetAsync($"/users/{_nonExistentUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> GetUser(WebApplicationFactory<Program> factory, string username, string password)
    {
        var client = await factory.CreateClient()
            .WithToken(username, password, TestSeedData.SomeApiClient.ClientId, TestSeedData.SomeApiClientSecret, $"{TestSeedData.SomeScope.Name} openid profile");

        // Get IS user info
        UserInfoRequest userInfoRequest = new()
        {
            Address = "/connect/userinfo",
            Token = client.DefaultRequestHeaders.Authorization?.Parameter
        };

        var userInfoResponse = await client.GetUserInfoAsync(userInfoRequest);
        userInfoResponse.IsError.ShouldBeFalse();
        userInfoResponse.Claims.ShouldNotBeEmpty();

        var subClaim = userInfoResponse.Claims.FirstOrDefault(c => c.Type == "sub");
        subClaim.ShouldNotBeNull();
        return Guid.Parse(subClaim.Value);
    }
}
