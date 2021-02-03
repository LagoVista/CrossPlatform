#define ENV_MASTER

using LagoVista.Client.Core.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.XPlat.WPF.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.AppLoader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
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

            SLWIOC.RegisterSingleton<IAppConfig>(new AppConfig());
            DeviceInfo.Register("WPF001");
            LagoVista.Xplat.WPF.Startup.Init();
            LagoVista.Client.Core.Startup.Init(serverInfo);
        }
    }
}
