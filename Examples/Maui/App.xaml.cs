using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace KGySoft.Drawing.Examples.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}
