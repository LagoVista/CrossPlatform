﻿using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.XPlat.WPF.NetStd.Core.Services;
using LagoVista.XPlat.WPF.Services;
using System.Windows.Threading;

namespace LagoVista.Core.WPF.PlatformSupport
{
    public class WPFDeviceServices
    {
        public static void Init(Dispatcher dispatcher)
        {
            SLWIOC.RegisterSingleton<IViewModelNavigation>(new ViewModelNavigation());
            SLWIOC.Register<IDispatcherServices>(new DispatcherServices(dispatcher));
            SLWIOC.Register<IPopupServices>(new PopupsService());
            SLWIOC.Register<IStorageService>(new StorageService());
            SLWIOC.Register<IDirectoryServices>(new DirectoryService());
            SLWIOC.Register<ILogger>(new DebugLogger());
            SLWIOC.Register<ITimerFactory>(new TimerFactory());
            SLWIOC.Register<IDeviceManager>(new DeviceManager());
            SLWIOC.Register<INetworkService>(new NetworkService());
            SLWIOC.Register<IProcessRunner>(new ProcessRunner());
        }
    }
}
