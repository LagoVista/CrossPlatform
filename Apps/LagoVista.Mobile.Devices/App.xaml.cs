using LagoVista.Core.ViewModels;

namespace LagoVista.Mobile.Devices
{
    public partial class App : Application
    {

        IViewModelNavigation _vmNav;

        public App(IViewModelNavigation vmNav)
        {
            _vmNav = vmNav;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell(_vmNav));
        }
    }
}