using LagoVista.Client.Core.Auth;
using LagoVista.Client.Core.Interfaces;
using LagoVista.Core.Geo;
using LagoVista.Core.IOC;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.Networking.WiFi;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.UWP.Networking;
using LagoVista.Core.UWP.Services;
using Windows.UI.Core;

namespace LagoVista.Core.UWP
{
    public static class Startup
    {
        public static void Init(Windows.UI.Xaml.Application app, CoreDispatcher dispatcher, string key)
        {
            SLWIOC.RegisterSingleton<ILogger>(new Loggers.AppCenterLogger(key));
            SLWIOC.RegisterSingleton<IDispatcherServices>(new DispatcherServices(dispatcher));
            SLWIOC.RegisterSingleton<IStorageService>(new StorageService());
            SLWIOC.RegisterSingleton<IPopupServices>(new PopupsService());

            SLWIOC.RegisterSingleton<IDeviceManager>(new DeviceManager());

            SLWIOC.RegisterSingleton<INetworkService>(new NetworkService());
            SLWIOC.Register<IImaging>(new Imaging());
            SLWIOC.RegisterSingleton<IBluetoothSerial>(new BluetoothSerial());
            SLWIOC.Register<IBindingHelper>(new BindingHelper());
            SLWIOC.RegisterSingleton<IGeoLocator>(new GeoLocator());

            SLWIOC.RegisterSingleton<ISSDPClient>(new SSDPClient());
            SLWIOC.RegisterSingleton<IWebServer>(new WebServer());
            SLWIOC.RegisterSingleton<IProcessOutputeWriter, ProcessOutputWriter>();

            SLWIOC.RegisterSingleton<IWiFiAdaptersService>(new WiFiAdaptersService());
            SLWIOC.RegisterSingleton<IWiFiNetworksService>(new WiFiNetworksService(dispatcher));

            SLWIOC.Register<ISSDPClient>(typeof(SSDPClient));
            SLWIOC.Register<IWebServer>(typeof(WebServer));
            SLWIOC.Register<ISecureStorage>(new SecureStorage());
            SLWIOC.Register<ISSDPServer>(new SSDPServer());

            SLWIOC.Register<IAppServices>(typeof(AppServices));

            SLWIOC.Register<IClipBoard>(new ClipBoard());

            SLWIOC.Register<ITimerFactory>(new TimerFactory());

            SLWIOC.Register<IDirectoryServices>(new DirectoryServices());
         
            IconFonts.IconFontSupport.RegisterFonts();
        }
    }
}
