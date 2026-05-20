using Starter.Bff.Configuration;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Starter.Bff.Proxy;

public class InternalApiKeyTransform : ITransformFactory
{
    private readonly string _internalKey;

    public InternalApiKeyTransform(ApiSettings settings) =>
        _internalKey = settings.InternalKey;

    public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
        => true;

    public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        context.AddRequestTransform(ctx =>
        {
            ctx.ProxyRequest.Headers.TryAddWithoutValidation("X-Internal-Key", _internalKey);
            return ValueTask.CompletedTask;
        });
        return true;
    }
}
