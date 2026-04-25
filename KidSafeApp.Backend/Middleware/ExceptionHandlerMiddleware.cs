using KidSafeApp.Backend.Models;
using System.Text.Json;

namespace KidSafeApp.Backend.Middleware;

public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = MapExceptionToErrorResponse(exception, context);
        context.Response.StatusCode = errorResponse.StatusCode;

        var json = JsonSerializer.Serialize(errorResponse);
        return context.Response.WriteAsync(json);
    }

    private static ErrorResponse MapExceptionToErrorResponse(Exception exception, HttpContext context)
    {
        var statusCode = exception switch
        {
            ArgumentException => 400,
            UnauthorizedAccessException => 401,
            KeyNotFoundException => 404,
            InvalidOperationException => 400,
            _ => 500
        };

        var message = GetUserFriendlyMessage(exception, statusCode);
        var traceId = context.TraceIdentifier;
        var path = context.Request.Path.Value;

        return new ErrorResponse(message, statusCode, path, traceId);
    }

    private static string GetUserFriendlyMessage(Exception exception, int statusCode)
    {
        if (statusCode < 500)
        {
            // Return the exception message for client errors (user can see it)
            return exception.Message ?? "An error occurred with your request.";
        }

        // For server errors, don't expose details in production
        return "An unexpected error occurred. Please try again later.";
    }
}

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
