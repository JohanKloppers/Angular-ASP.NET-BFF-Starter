using Starter.Bff.Endpoints;

namespace Starter.Bff.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapBffEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("");

        endpoints.MapAuthEndpoints();
        endpoints.MapUserEndpoints();
        endpoints.MapHealthEndpoints();
        endpoints.MapReverseProxy();

        return app;
    }
}
