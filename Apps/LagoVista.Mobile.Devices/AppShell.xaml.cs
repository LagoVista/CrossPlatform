using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.ViewModels;
using LagoVista.XPlat.Maui.Services;

namespace LagoVista.Mobile.Devices
{
    public partial class AppShell : Shell
    {
        public AppShell(IViewModelNavigation nav)
        {
            var vmNav = nav as ViewModelNavigation;
            vmNav.SetHost(this.Navigation);
            InitializeComponent();
            vmNav.SetAsNewRootAsync<SplashViewModel>();
        }
    }
}
