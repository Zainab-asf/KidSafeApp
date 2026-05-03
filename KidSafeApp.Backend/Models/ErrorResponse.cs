namespace KidSafeApp.Backend.Models;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Path { get; set; }
    public string? TraceId { get; set; }

    public ErrorResponse()
    {
    }

    public ErrorResponse(string message, int statusCode, string? path = null, string? traceId = null)
    {
        Message = message;
        StatusCode = statusCode;
        Path = path;
        TraceId = traceId;
        Timestamp = DateTime.UtcNow;
    }

    public static ErrorResponse BadRequest(string message = "Bad request.") =>
        new(message, 400);

    public static ErrorResponse Unauthorized(string message = "Unauthorized access.") =>
        new(message, 401);

    public static ErrorResponse Forbidden(string message = "Access forbidden.") =>
        new(message, 403);

    public static ErrorResponse NotFound(string message = "Resource not found.") =>
        new(message, 404);

    public static ErrorResponse Conflict(string message = "Resource conflict.") =>
        new(message, 409);

    public static ErrorResponse InternalServerError(string message = "An unexpected error occurred.") =>
        new(message, 500);
}
