using Android.Content;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.XPlat.Droid.Services;
using LagoVista.Core;
using LagoVista.Client.Core.Auth;
using LagoVista.Client.Core;
using LagoVista.Client.Core.Net;
using LagoVista.Client.Core.Interfaces;

namespace LagoVista.XPlat.Droid
{
    public static class Startup
    {
        private static bool _isInitialized = false;

        public static void Init(Context context, string key)
        {
            if (!_isInitialized)
            {
                var logger = new Loggers.AppCenterLogger(key);
                SLWIOC.RegisterSingleton<ILogger>(logger);

                var popupService = new LagoVista.XPlat.Core.Services.PopupServices();

                SLWIOC.RegisterSingleton<IBluetoothSerial>(new BluetoothSerial(logger, context, popupService));
                SLWIOC.RegisterSingleton<IProcessOutputWriter>(new Services.ProcessWriter(context));

                SLWIOC.RegisterSingleton<IStorageService>(new StorageService());
                SLWIOC.RegisterSingleton<IDispatcherServices>(new DispatcherServices(context));

                SLWIOC.Register<IPopupServices>(popupService);
                SLWIOC.Register<INetworkService>(new NetworkService());
                SLWIOC.Register<ITCPClient, Services.TCPClient>();
                SLWIOC.Register<IUDPClient, Services.UDPClient>();

                SLWIOC.Register<IAppServices, Services.AppService>();
                SLWIOC.Register<IWebSocket, Services.WebSocket>();
                SLWIOC.Register<ISecureStorage>(new SecureStorage());
                SLWIOC.Register<IClipBoard, Services.ClipBoard>();
                

                IconFonts.IconFontSupport.RegisterFonts();
                _isInitialized = true;
            }
        }
    }
}