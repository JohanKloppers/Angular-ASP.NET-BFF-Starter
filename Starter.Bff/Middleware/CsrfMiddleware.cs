namespace Starter.Bff.Middleware;

// Double-submit cookie CSRF protection.
// Angular reads the X-CSRF-TOKEN cookie (HttpOnly=false) and echoes it back
// as the X-CSRF-TOKEN request header on all mutating requests.
public class CsrfMiddleware
{
    private readonly RequestDelegate _next;
    private const string CsrfCookieName = "X-CSRF-TOKEN";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";

    // Login and register are anonymous endpoints — no CSRF cookie exists yet.
    private static readonly HashSet<string> ExemptPaths = ["/login", "/register", "/health"];

    public CsrfMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method))
        {
            if (!context.Request.Cookies.ContainsKey(CsrfCookieName))
                SetNewCsrfCookie(context);
        }
        else if (HttpMethods.IsPost(context.Request.Method) ||
                 HttpMethods.IsPut(context.Request.Method) ||
                 HttpMethods.IsDelete(context.Request.Method) ||
                 HttpMethods.IsPatch(context.Request.Method))
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var isExempt = ExemptPaths.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase));

            if (!isExempt)
            {
                var cookie = context.Request.Cookies[CsrfCookieName];
                var header = context.Request.Headers[CsrfHeaderName].ToString();

                if (string.IsNullOrEmpty(cookie) || !string.Equals(cookie, header, StringComparison.Ordinal))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("CSRF token validation failed.");
                    return;
                }
            }
        }

        await _next(context);
    }

    public static void SetNewCsrfCookie(HttpContext context)
    {
        var token = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CsrfCookieName, token, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
        });
    }
}
