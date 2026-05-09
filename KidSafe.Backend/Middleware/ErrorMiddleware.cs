namespace KidSafe.Backend.Middleware;

public sealed class ErrorMiddleware
{
    private readonly RequestDelegate          _next;
    private readonly ILogger<ErrorMiddleware> _logger;

    public ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled {ExType} on {Method} {Path}",
                ex.GetType().Name, ctx.Request.Method, ctx.Request.Path);

            if (!ctx.Response.HasStarted)
            {
                ctx.Response.StatusCode  = 500;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(
                    new { error = "An unexpected error occurred.", traceId = ctx.TraceIdentifier });
            }
        }
    }
}

public static class ErrorMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ErrorMiddleware>();
}
