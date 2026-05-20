using System.Security.Claims;
using Starter.Bff.Services;

namespace Starter.Bff.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/user", (ClaimsPrincipal user, IUserService userService) =>
            Results.Ok(userService.GetUserInfo(user))
        ).RequireAuthorization();

        return app;
    }
}
