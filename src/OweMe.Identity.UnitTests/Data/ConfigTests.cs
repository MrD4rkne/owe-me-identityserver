using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.Extensions.Configuration;
using OweMe.Identity.Server.Setup;
using Shouldly;

namespace OweMe.Identity.UnitTests.Data;

public class ConfigTests
{
    private const string OWEME_API_SECRET = "oweme-client";
    private const string OWEME_WEB_SECRET = "web-secret";
    private readonly string[] OWEME_WEB_REDIRECT_URIS = [ "http://localhost:5000/signin-oidc", "http://localhost:6000/signin-oidc" ];
    private readonly string[] OWEME_WEB_CALLBACK_URIS = [ "http://localhost:5000/signout-callback-oidc", "http://localhost:6000/signout-callback-oidc" ];

    private readonly TestUser[] OWEME_TEST_USERS =
    [
        new()
        {
            SubjectId = "1",
            Username = "testuser",
            Password = "password"
        },
        new()
        {
            SubjectId = "2",
            Username = "testuser2",
            Password = "password2"
        }
    ];
    
    
    private readonly Config _config;

    public ConfigTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "OweMe:Api:ClientSecret", OWEME_API_SECRET },
            { "OweMe:Web:ClientSecret", OWEME_WEB_SECRET },
            { "OweMe:Web:RedirectUris", string.Join(",", OWEME_WEB_REDIRECT_URIS) },
            { "OweMe:Web:CallbackUris", string.Join(",", OWEME_WEB_CALLBACK_URIS) },
        };

        for (int i = 0; i < OWEME_TEST_USERS.Length; i++)
        {
            inMemorySettings.Add($"OweMe:TestUsers:{i}:SubjectId", OWEME_TEST_USERS[i].SubjectId);
            inMemorySettings.Add($"OweMe:TestUsers:{i}:Username", OWEME_TEST_USERS[i].Username);
            inMemorySettings.Add($"OweMe:TestUsers:{i}:Password", OWEME_TEST_USERS[i].Password);
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _config = new Config(configuration);
    }

    [Fact]
    public void IdentityResources_ShouldContainExpectedResources()
    {
        // Arrange
        var expectedIdentityResources = new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new("user", [JwtClaimTypes.Email])
        };
        
        // Act
        var identityResources = _config.IdentityResources.ToList();

        // Assert
        identityResources.ShouldBeEquivalentTo(expectedIdentityResources);
    }

    [Fact]
    public void ApiScopes_ShouldContainExpectedScopes()
    {
        // Arrange
        var expectedScopes = new List<ApiScope>
        {
            new("oweme-api", "Owe-Me API")
        };
        
        // Act
        var apiScopes = _config.ApiScopes.ToList();

        // Assert
        apiScopes.ShouldBeEquivalentTo(expectedScopes);
    }

    [Fact]
    public void Clients_ShouldContainExpectedClients()
    {
        // Act
        var clients = _config.Clients.ToList();

        // Assert
        var apiClient = clients.FirstOrDefault(c => c.ClientId == "oweme-client");
        apiClient.ShouldNotBeNull();
        apiClient.ClientSecrets.ShouldContain(secret => secret.Value == OWEME_API_SECRET.Sha256());
        apiClient.AllowedScopes.ShouldBeEquivalentTo(new[] { "oweme-api" });
        apiClient.AllowedGrantTypes.ShouldAllBe(x=> x == OidcConstants.GrantTypes.Password || x == OidcConstants.GrantTypes.ClientCredentials);
        apiClient.AlwaysSendClientClaims.ShouldBeTrue();
        
        var webClient = clients.FirstOrDefault(c => c.ClientId == "oweme-web");
        webClient.ShouldNotBeNull();
        webClient.ClientSecrets.ShouldContain(secret => secret.Value == OWEME_WEB_SECRET.Sha256());
        webClient.AllowedGrantTypes.ShouldBeEquivalentTo(new[] { GrantTypes.Code });
        webClient.RedirectUris.ShouldBeEquivalentTo(OWEME_WEB_REDIRECT_URIS);
        webClient.PostLogoutRedirectUris.ShouldBeEquivalentTo(OWEME_WEB_CALLBACK_URIS);
        webClient.AllowedScopes.ShouldBeEquivalentTo(new[] { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile });
    }

    [Fact]
    public void Users_ShouldContainExpectedTestUsers()
    {
        var users = _config.Users;

        Assert.Contains(users, user => user.Username == "testuser" && user.Password == "password");
    }
}