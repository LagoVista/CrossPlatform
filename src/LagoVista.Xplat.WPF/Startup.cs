using LagoVista.Client.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.XPlat.WPF.Services;

namespace LagoVista.Xplat.WPF
{
    public class Startup
    {
        public static void Init()
        {
            SLWIOC.RegisterSingleton<IStorageService>(new StorageService());
            SLWIOC.RegisterSingleton<INetworkService>(new NetworkService());
            SLWIOC.RegisterSingleton<ILogger>(new Logger());
        }
    }
}
