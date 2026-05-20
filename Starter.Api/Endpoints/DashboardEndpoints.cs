using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Starter.Api.Models;

namespace Starter.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/stats", async (UserManager<ApplicationUser> userManager) =>
        {
            var today = DateTime.UtcNow.Date;
            var totalUsers = await userManager.Users.CountAsync();
            var registeredToday = await userManager.Users.CountAsync(u => u.CreatedAt >= today);

            return Results.Ok(new { totalUsers, registeredToday });
        });

        return app;
    }
}
