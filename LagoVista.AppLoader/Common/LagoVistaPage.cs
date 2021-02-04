using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LagoVista.AppLoader.Common
{
    public class LagoVistaPage : Page
    {
        public LagoVistaPage()
        {
            this.Loaded += LagoVistaPage_Loaded;
        }

        private async void LagoVistaPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ClearNavStack)
            {
                ClearHistory();
            }

            var vm = DataContext as ViewModelBase;
            await vm.InitAsync();
        }

        public void ClearHistory()
        {
            if (!this.NavigationService.CanGoBack && !this.NavigationService.CanGoForward)
            {
                return;
            }

            var entry = this.NavigationService.RemoveBackEntry();
            while (entry != null)
            {
                entry = this.NavigationService.RemoveBackEntry();
            }
        }


        public bool ClearNavStack { get; set; } = false;
    }
}
