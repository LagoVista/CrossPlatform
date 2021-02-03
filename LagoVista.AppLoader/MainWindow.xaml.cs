using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;

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
            _navigation.RegisterPage<SplashViewModel>("/Views/Splash.xaml");
            _navigation.RegisterPage<DFUViewModel>("/Views/DFUView.xaml");
            _navigation.RegisterPage<LoginViewModel>("/Views/LoginPage.xaml");
            SLWIOC.RegisterSingleton<IViewModelNavigation>(_navigation);

            Source = new System.Uri("/views/Splash.xaml", System.UriKind.Relative);

            this.LoadCompleted += MainWindow_LoadCompleted;            
        }

        private async void MainWindow_LoadCompleted(object sender, NavigationEventArgs e)
        {
            var obj = this.Content as Page;
            var vm = SLWIOC.CreateForType(typeof(SplashViewModel)) as ViewModelBase;
            obj.DataContext = vm;
            await vm.InitAsync();            
        }
    }
}
