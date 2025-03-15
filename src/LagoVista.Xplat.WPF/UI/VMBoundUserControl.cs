using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LagoVista.XPlat
{
    public class VMBoundUserControl<VMType> : UserControl where VMType : class, IViewModel
    {
        private bool _initialized;

        public VMBoundUserControl()
        {
            this.Loaded += VMBoundUserControl_Loaded;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (SLWIOC.TryResolve(typeof(VMType), out object value))
                {
                    ViewModel = value as VMType;
                    DataContext = ViewModel;
                }
                else
                {
                    var vm = SLWIOC.Create<VMType>();
                    ViewModel = vm as VMType;
                    DataContext = ViewModel;
                }
            }

        }

        private async void VMBoundUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_initialized)
                return;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                await ViewModel.InitAsync();
            }

            _initialized = true;
        }

       
        public VMType ViewModel { get; private set; }
    }
}
