using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Starter.Bff.Middleware;
using Starter.Bff.Services;

namespace Starter.Bff.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (
            LoginRequest request,
            IApiAuthService apiAuth,
            HttpContext context) =>
        {
            var user = await apiAuth.LoginAsync(request.Email, request.Password);
            if (user is null) return Results.Unauthorized();

            await SignInAsync(context, user.Id, user.Email, user.FirstName, user.LastName);
            CsrfMiddleware.SetNewCsrfCookie(context);
            return Results.Ok(new { user.Email, user.FirstName, user.LastName });
        }).AllowAnonymous();

        app.MapPost("/register", async (
            RegisterRequest request,
            IApiAuthService apiAuth,
            HttpContext context) =>
        {
            var user = await apiAuth.RegisterAsync(
                request.Email, request.Password, request.FirstName, request.LastName);

            if (user is null)
                return Results.BadRequest(new { error = "Registration failed. The email may already be in use." });

            await SignInAsync(context, user.Id, user.Email, user.FirstName, user.LastName);
            CsrfMiddleware.SetNewCsrfCookie(context);
            return Results.Ok(new { user.Email, user.FirstName, user.LastName });
        }).AllowAnonymous();

        app.MapGet("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok();
        }).RequireAuthorization();

        return app;
    }

    private static Task SignInAsync(
        HttpContext context, Guid userId, string email, string firstName, string lastName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("sub", userId.ToString()),
            new("email", email),
            new("name", $"{firstName} {lastName}"),
            new("firstName", firstName),
            new("lastName", lastName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
}
