using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.XPlat.WPF.Services;
using System.Windows;

namespace LagoVista.AppLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DFUViewModel _dfuViewModel;

        public MainWindow()
        {
            InitializeComponent();

            _dfuViewModel = new DFUViewModel(new ProcessOutputWriter(), new AppServices());

            DataContext = _dfuViewModel;
        }


    }
}
