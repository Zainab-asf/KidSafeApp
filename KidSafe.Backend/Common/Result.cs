namespace KidSafe.Backend.Common;

/// <summary>
/// Typed result for service layer — avoids null returns and ambiguous exceptions.
/// </summary>
public sealed class Result<T>
{
    public bool    IsSuccess { get; }
    public T?      Value     { get; }
    public string? Error     { get; }
    public int     StatusCode { get; }

    private Result(bool ok, T? value, string? error, int code)
    {
        IsSuccess  = ok;
        Value      = value;
        Error      = error;
        StatusCode = code;
    }

    public static Result<T> Ok(T value)                           => new(true,  value,   null,  200);
    public static Result<T> Fail(string error, int code = 400)   => new(false, default, error, code);
    public static Result<T> NotFound(string error = "Not found") => new(false, default, error, 404);
    public static Result<T> Conflict(string error)               => new(false, default, error, 409);
}

public static class Result
{
    public static Result<T> Ok<T>(T value)                         => Result<T>.Ok(value);
    public static Result<T> Fail<T>(string error, int code = 400)  => Result<T>.Fail(error, code);
}
