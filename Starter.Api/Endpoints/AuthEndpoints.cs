using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Starter.Api.Filters;
using Starter.Api.Models;

namespace Starter.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (
            RegisterRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var existing = await userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
                return Results.Conflict(new { error = "An account with that email already exists." });

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return Results.BadRequest(new { errors });
            }

            return Results.Created($"/api/auth/user/{user.Id}", ToResponse(user));
        })
        .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
        .RequireRateLimiting("auth");

        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null) return Results.Unauthorized();

            var valid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!valid) return Results.Unauthorized();

            return Results.Ok(ToResponse(user));
        })
        .AddEndpointFilter<ValidationFilter<LoginRequest>>()
        .RequireRateLimiting("auth");

        app.MapGet("/api/auth/user/{id:guid}", async (
            Guid id,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            return user is null ? Results.NotFound() : Results.Ok(ToResponse(user));
        });

        return app;
    }

    private static UserResponse ToResponse(ApplicationUser user) =>
        new(user.Id, user.Email!, user.FirstName, user.LastName);

    public record RegisterRequest(
        [Required, EmailAddress, MaxLength(256)] string Email,
        [Required, MinLength(8), MaxLength(100)] string Password,
        [Required, MaxLength(50)] string FirstName,
        [Required, MaxLength(50)] string LastName);

    public record LoginRequest(
        [Required, EmailAddress] string Email,
        [Required] string Password);

    public record UserResponse(Guid Id, string Email, string FirstName, string LastName);
}
