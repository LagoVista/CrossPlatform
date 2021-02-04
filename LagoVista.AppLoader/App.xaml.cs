#define ENV_MASTER

using LagoVista.Client.Core;
using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.XPlat.WPF.Services;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace LagoVista.AppLoader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void InitApp()
        {
#if ENV_MASTER
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "api.nuviot.com",
            };
#elif ENV_DEV
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "dev-api.nuviot.com",
            };
#elif ENV_LOCAL
            var serverInfo = new ServerInfo()
            {
                SSL = false,
                RootUrl = "localhost:5001",
            };

#elif ENV_STAGE
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "test-api.nuviot.com",
            };            
#endif

            var appConfig = new AppConfig();

            SLWIOC.RegisterSingleton<IBluetoothSerial>(new Services.BluetoothSerial());
            SLWIOC.RegisterSingleton<IAppConfig>(appConfig);
            SLWIOC.RegisterSingleton<IPopupServices, Services.PopupService>();
            SLWIOC.RegisterSingleton<IClientAppInfo>(new ClientAppInfo());
            LagoVista.Core.WPF.IconFonts.IconFontSupport.RegisterFonts();
            DeviceInfo.Register("WPF001");
            LagoVista.Xplat.WPF.Startup.Init(appConfig);
            LagoVista.Client.Core.Startup.Init(serverInfo);
        }

        public App()
        {
            InitApp();
        }
    }
}
