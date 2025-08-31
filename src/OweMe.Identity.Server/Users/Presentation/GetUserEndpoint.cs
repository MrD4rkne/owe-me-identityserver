using Microsoft.AspNetCore.Mvc;
using OweMe.Identity.Server.Users.Domain;

namespace OweMe.Identity.Server.Users.Presentation;

public static class GetUserEndpoint
{
    private const string PolicyName = "UserLookup";

    // public static void RegisterPolicies(this AuthorizationOptions options)
    // {
    //     options.AddPolicy(PolicyName, policy =>
    //     {
    //         policy.RequireClaim("scope", "users");
    //     });
    // }
    //
    public static void MapGetUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{userId}", GetUserById)
            .WithTags("Users")
            .Produces<User>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(IdentityServerConstants.LocalApi.PolicyName);
    }

    public static async Task<IResult> GetUserById(string userId, [FromServices] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByIdAsync(userId, cancellationToken);
        if (user is null || user.UserName is null || user.Email is null)
        {
            return Results.NotFound();
        }

        var result = new User
        {
            Sub = user.Id,
            Email = user.Email,
            UserName = user.UserName
        };
        return Results.Ok(result);
    }
}