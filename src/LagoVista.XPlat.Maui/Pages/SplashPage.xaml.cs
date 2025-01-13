using LagoVista.Client.Core.ViewModels;

namespace LagoVista.XPlat.Maui.Pages;

public partial class SplashPage : PageBase
{
	public SplashPage(SplashViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}