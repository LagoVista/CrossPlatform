using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;
using LagoVista.AppLoader.Views;

namespace LagoVista.AppLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        private Services.ViewModelNavigation _navigation;

        public MainWindow()
        {
            InitializeComponent();

            _navigation = new Services.ViewModelNavigation(this);
            _navigation.RegisterPage<DeviceReposViewModel, DeviceReposView>();
            _navigation.RegisterPage<DevicesViewModel, DevicesView>();
            _navigation.RegisterPage<DeviceViewModel, DeviceView>();
            _navigation.RegisterPage<SplashViewModel, Splash>();
            _navigation.RegisterPage<DFUViewModel, DFUView>();
            _navigation.RegisterPage<LoginViewModel, LoginPage>();
            SLWIOC.RegisterSingleton<IViewModelNavigation>(_navigation);

            var splashPage = new Splash();
            var vm = SLWIOC.CreateForType(typeof(SplashViewModel)) as ViewModelBase;
            splashPage.DataContext = vm;
            Navigate(splashPage);
        }
    }
}
