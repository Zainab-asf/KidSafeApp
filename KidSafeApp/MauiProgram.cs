using Microsoft.Extensions.Logging;
using KidSafeApp.Services;
using KidSafeApp.States;
using System.Net.Http;

namespace KidSafeApp
{
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

            builder.Services.AddScoped<RoleState>();
            builder.Services.AddScoped<AccountManager>();
            builder.Services.AddScoped<AdminSessionState>();

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5065/")
            });

            // Chat service for Messages.razor and dashboards
            builder.Services.AddScoped<ChatService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
