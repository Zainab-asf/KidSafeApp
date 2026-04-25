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
            builder.Services.AddScoped<AuthenticationState>();

            builder.Services.AddScoped(sp =>
            {
                var baseAddress = DeviceInfo.Platform == DevicePlatform.Android
                    ? "http://10.0.2.2:5065/"
                    : "http://localhost:5065/";

                return new HttpClient
                {
                    BaseAddress = new Uri(baseAddress)
                };
            });

            // Chat service for Messages.razor and dashboards
            builder.Services.AddScoped<ChatService>();
            builder.Services.AddScoped<AdminUsersApiClient>();
            
            // Add missing services from Phase 2
            builder.Services.AddScoped<ErrorService>();
            builder.Services.AddScoped<MessageApiClient>();
            builder.Services.AddScoped<HubConnectionService>();
            builder.Services.AddScoped<AuthenticationService>();
            builder.Services.AddScoped<ParentDashboardApiClient>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
