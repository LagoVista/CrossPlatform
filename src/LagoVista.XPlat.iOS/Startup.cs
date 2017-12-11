using UIKit;
using LagoVista.XPlat.iOS.Services;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core;
using LagoVista.Core.IOC;
using LagoVista.XPlat.Core.Services;
using LagoVista.Client.Core.Net;
using LagoVista.MQTT.Core;
using LagoVista.Client.Core.Auth;

namespace LagoVista.XPlat.iOS
{
    public static class Startup
    {
        public static void Init(UIApplication app, string key)
        {
            SLWIOC.RegisterSingleton<ILogger>(new Loggers.MobileCenterLogger(key));
            SLWIOC.Register<IStorageService>(new StorageService());
            SLWIOC.Register<INetworkService>(new NetworkService());
            SLWIOC.Register<IDeviceInfo>(new DeviceInfo());
            SLWIOC.Register<ISecureStorage, SecureStorage>();
            SLWIOC.Register<IPopupServices>(new PopupServices());
            SLWIOC.Register<IWebSocket,Services.WebSocket>();
            SLWIOC.Register<IDispatcherServices>(new DispatcherService(app));
            SLWIOC.Register<IMqttNetworkChannel, MqttNetworkChannel>();

            IconFonts.IconFontSupport.RegisterFonts();
        }
    }
}