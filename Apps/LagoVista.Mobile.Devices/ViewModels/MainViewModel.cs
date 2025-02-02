using LagoVista.Client.Core.Forms;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.DeviceSetup;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.DeviceManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Mobile.Devices.ViewModels
{
    public class MainViewModel : AppViewModelBase
    {
        public MainViewModel()
        {
            ShowInstancesCommand = new RelayCommand(() => ShowView<MyDevicesViewModel>());
            ShowDeviceRepositoryCommand = new RelayCommand(() => ShowView<ListResponseViewModel<DeviceRepositorySummary>>());
        }

        public RelayCommand ShowInstancesCommand { get; }
        public RelayCommand ShowDeviceRepositoryCommand { get; }
    }
}
