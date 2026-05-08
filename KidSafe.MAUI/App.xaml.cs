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
			Title   = "KidSafe – Safe Chat",
			Width   = 430,
			Height  = 900,
			MinimumWidth  = 380,
			MinimumHeight = 700,
		};
	}
}
