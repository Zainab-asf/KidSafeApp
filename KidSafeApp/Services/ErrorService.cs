namespace KidSafeApp.Services;

public sealed class ErrorService
{
    public string? LastError { get; private set; }

    public void Set(string message) => LastError = message;

    public void Clear() => LastError = null;
}

