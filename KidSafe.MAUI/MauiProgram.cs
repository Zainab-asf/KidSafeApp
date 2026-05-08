using Microsoft.Extensions.Logging;
using KidSafe.MAUI.Services;

namespace KidSafe.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // ── HTTP client pointing to local backend ─────────────────────
        builder.Services.AddHttpClient<ApiService>(c =>
            c.BaseAddress = new Uri("http://localhost:5000/"));

        // ── App services ──────────────────────────────────────────────
        builder.Services.AddSingleton<AuthStateService>();
        builder.Services.AddSingleton<ChatHubService>();
        builder.Services.AddSingleton<FcmService>();

        return builder.Build();
    }
}
