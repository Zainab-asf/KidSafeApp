using Microsoft.AspNetCore.Components.WebView.Maui;

namespace KidSafeApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(Routes)
            });
        }
    }
}
