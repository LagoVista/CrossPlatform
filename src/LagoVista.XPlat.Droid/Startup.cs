using Android.Content;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.XPlat.Droid.Services;
using LagoVista.Core;

namespace LagoVista.XPlat.Droid
{
    public static class Startup
    {
        public static void Init(Context context, string key)
        {
            SLWIOC.RegisterSingleton<ILogger>(new Loggers.MobileCenterLogger(key));
            SLWIOC.Register<IStorageService>(new StorageService());
            SLWIOC.Register<IPopupServices>(new LagoVista.XPlat.Core.Services.PopupServices());
            SLWIOC.Register<INetworkService>(new NetworkService());
            SLWIOC.Register<IDeviceInfo>(new DeviceInfo());
            SLWIOC.Register<IDispatcherServices>(new DispatcherServices(context));

            IconFonts.IconFontSupport.RegisterFonts();

        }
    }
}