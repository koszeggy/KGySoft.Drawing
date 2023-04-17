using Microsoft.Maui.Controls;

namespace KGySoft.Drawing.Examples.SkiaSharp.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
