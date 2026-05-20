using Starter.Bff.Configuration;

namespace Starter.Bff.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersConfig _config;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersConfig config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var header in _config.Headers)
            context.Response.Headers.Append(header.Key, header.Value);

        await _next(context);
    }
}
