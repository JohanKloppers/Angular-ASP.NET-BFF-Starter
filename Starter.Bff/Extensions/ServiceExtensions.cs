using Starter.Bff.Configuration;
using Starter.Bff.Services;
using Yarp.ReverseProxy.Transforms;

namespace Starter.Bff.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddBffServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton(
            config.GetSection(SecurityHeadersConfig.SectionName).Get<SecurityHeadersConfig>()!);

        var apiSettings = config.GetSection(ApiSettings.SectionName).Get<ApiSettings>()!;
        services.AddSingleton(apiSettings);

        services.AddHttpClient<IApiAuthService, ApiAuthService>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.DefaultRequestHeaders.Add("X-Internal-Key", apiSettings.InternalKey);
        });

        services.AddScoped<IUserService, UserService>();

        services.AddReverseProxy()
            .LoadFromConfig(config.GetSection("ReverseProxy"))
            .AddTransforms(ctx =>
            {
                ctx.AddRequestTransform(req =>
                {
                    req.ProxyRequest.Headers.TryAddWithoutValidation("X-Internal-Key", apiSettings.InternalKey);
                    return ValueTask.CompletedTask;
                });
            });

        return services;
    }
}
