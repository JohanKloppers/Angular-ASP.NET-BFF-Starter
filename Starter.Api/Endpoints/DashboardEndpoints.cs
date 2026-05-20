using Dapper;
using Starter.Api.Data;

namespace Starter.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/stats", async (IDbConnectionFactory dbFactory) =>
        {
            var today = DateTime.UtcNow.Date;
            using var conn = dbFactory.CreateConnection();
            var totalUsers = await conn.ExecuteScalarAsync<long>(@"SELECT COUNT(*) FROM ""AspNetUsers""");
            var registeredToday = await conn.ExecuteScalarAsync<long>(
                @"SELECT COUNT(*) FROM ""AspNetUsers"" WHERE ""CreatedAt"" >= @today", new { today });

            return Results.Ok(new { totalUsers, registeredToday });
        });

        return app;
    }
}
