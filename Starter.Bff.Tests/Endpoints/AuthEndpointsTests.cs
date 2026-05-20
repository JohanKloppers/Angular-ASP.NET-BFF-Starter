using Starter.Bff.Middleware;

namespace Starter.Bff.Tests.Middleware;

public class CsrfMiddlewareTests
{
    [Fact]
    public async Task Get_WithoutCsrfCookie_SetsCsrfCookie()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Response.Body = new MemoryStream();

        var middleware = new CsrfMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        Assert.True(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task Get_WithExistingCsrfCookie_DoesNotOverwriteCookie()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers.Cookie = "X-CSRF-TOKEN=existing-token";
        context.Response.Body = new MemoryStream();

        var middleware = new CsrfMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task Post_ToLoginPath_PassesThrough()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/login";
        context.Response.Body = new MemoryStream();

        var called = false;
        var middleware = new CsrfMiddleware(_ => { called = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);

        Assert.True(called);
        Assert.NotEqual(403, context.Response.StatusCode);
    }

    [Fact]
    public async Task Post_ToRegisterPath_PassesThrough()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/register";
        context.Response.Body = new MemoryStream();

        var called = false;
        var middleware = new CsrfMiddleware(_ => { called = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);

        Assert.True(called);
        Assert.NotEqual(403, context.Response.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutCsrfToken_Returns403()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/some-action";
        context.Response.Body = new MemoryStream();

        var middleware = new CsrfMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        Assert.Equal(403, context.Response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMismatchedCsrfToken_Returns403()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/some-action";
        context.Request.Headers.Cookie = "X-CSRF-TOKEN=token-a";
        context.Request.Headers["X-CSRF-TOKEN"] = "token-b";
        context.Response.Body = new MemoryStream();

        var middleware = new CsrfMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        Assert.Equal(403, context.Response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMatchingCsrfToken_PassesThrough()
    {
        var token = "valid-csrf-token-abc";
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/some-action";
        context.Request.Headers.Cookie = $"X-CSRF-TOKEN={token}";
        context.Request.Headers["X-CSRF-TOKEN"] = token;
        context.Response.Body = new MemoryStream();

        var called = false;
        var middleware = new CsrfMiddleware(_ => { called = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);

        Assert.True(called);
        Assert.NotEqual(403, context.Response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithMatchingCsrfToken_PassesThrough()
    {
        var token = "valid-csrf-token-xyz";
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Delete;
        context.Request.Path = "/api/some-resource";
        context.Request.Headers.Cookie = $"X-CSRF-TOKEN={token}";
        context.Request.Headers["X-CSRF-TOKEN"] = token;
        context.Response.Body = new MemoryStream();

        var called = false;
        var middleware = new CsrfMiddleware(_ => { called = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);

        Assert.True(called);
    }
}
