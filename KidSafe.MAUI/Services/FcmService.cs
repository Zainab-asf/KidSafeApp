namespace KidSafe.MAUI.Services;

/// <summary>
/// FCM stub for MAUI Windows — Firebase Web Push is browser-only.
/// On Windows, push notifications can be wired via WNS if needed.
/// For now, this is a no-op so all pages compile without changes.
/// </summary>
public class FcmService : IDisposable
{
#pragma warning disable CS0067
    public event Action<string, string, string>? OnForegroundMessage;
#pragma warning restore CS0067

    public Task InitAsync() => Task.CompletedTask;

    public void Dispose() { }
}
