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
                SLWIOC.RegisterSingleton<ILogger>(new Loggers.AppCenterLogger(key));
                SLWIOC.Register<IStorageService>(new StorageService());
                SLWIOC.Register<IBluetoothSerial, BluetoothSerial>();
                SLWIOC.Register<IPopupServices>(new LagoVista.XPlat.Core.Services.PopupServices());
                SLWIOC.Register<INetworkService>(new NetworkService());
                SLWIOC.Register<ITCPClient, Services.TCPClient>();
                SLWIOC.Register<IUDPClient, Services.UDPClient>();
                SLWIOC.Register<IWebSocket, Services.WebSocket>();
                SLWIOC.Register<ISecureStorage>(new SecureStorage());
                SLWIOC.Register<IClipBoard, Services.ClipBoard>();
                SLWIOC.Register<IDispatcherServices>(new DispatcherServices(context));

                IconFonts.IconFontSupport.RegisterFonts();
                _isInitialized = true;
            }
        }
    }
}