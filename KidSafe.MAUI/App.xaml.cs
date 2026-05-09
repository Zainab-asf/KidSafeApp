namespace KidSafe.MAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage())
		{
			Title        = "KidSafe – Safe Chat",
			Width        = 1100,
			Height       = 720,
			MinimumWidth  = 600,
			MinimumHeight = 500,
		};
	}
}
