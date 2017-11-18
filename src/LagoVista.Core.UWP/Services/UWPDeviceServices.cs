using LagoVista.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.PlatformSupport;
using Windows.UI.Core;

namespace LagoVista.Core.UWP.Services
{
    public static class UWPDeviceServices
    {
        public static void Init(CoreDispatcher dispatcher)
        {
            SLWIOC.RegisterSingleton<IDispatcherServices>(new DispatcherServices(dispatcher));
            SLWIOC.RegisterSingleton<IStorageService>(new StorageService());
            SLWIOC.RegisterSingleton<IPopupServices>(new PopupsService());
     
            SLWIOC.RegisterSingleton<IDeviceManager>(new DeviceManager());

            SLWIOC.RegisterSingleton<INetworkService>(new NetworkService());
            SLWIOC.Register<IImaging>(new Imaging());
            SLWIOC.Register<IBindingHelper>(new BindingHelper());

            SLWIOC.RegisterSingleton<ISSDPClient>(new SSDPClient());
            SLWIOC.RegisterSingleton<IWebServer>(new WebServer());

            SLWIOC.Register<ISSDPClient>(typeof(SSDPClient));
            SLWIOC.Register<IWebServer>(typeof(WebServer));
            SLWIOC.Register<ISSDPServer>(new SSDPServer());

            SLWIOC.Register<ITimerFactory>(new TimerFactory());

            SLWIOC.Register<IDirectoryServices>(new DirectoryServices());
        }
    }
}
