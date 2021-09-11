//#define ENV_LOCALDEV
//#define ENV_LOCALDEV
//#define ENV_DEV
#define ENV_MASTER

using System;
using Xamarin.Forms;
using LagoVista.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.IOC;
using LagoVista.Client.Core;
using LagoVista.XPlat.Core.Services;
using SeaWolf.ViewModels;
using LagoVista.Client.Devices;
using LagoVista.Core.ViewModels;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.PlatformSupport;
using LagoVista.XPlat.Core.Views;
using LagoVista.Core.Models;
using LagoVista;
using LagoVista.Client.Core.Interfaces;

namespace SeaWolf
{
    public partial class App : Application
    {
        AppConfig _appConfig;

        public static App Instance { get; private set; }

        public App()
        {
            Device.SetFlags(new string[] { "Shapes_Experimental" });
            InitializeComponent();

            App.Instance= this;

            InitServices();
        }

        private void InitServices()
        {
            _appConfig = new AppConfig();

#if ENV_STAGE
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "api.nuviot.com",
            };

            make sure it doesn't compile....add in a device repo id
#elif ENV_DEV
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "dev-api.nuviot.com",
            };

            make sure it doesn't compile....add in a device repo id
#elif ENV_LOCALDEV
            var serverInfo = new ServerInfo()
            {
                SSL = false,
                RootUrl = "localhost:5001",
            };

            make sure it doesn't compile....add in a device repo id
#elif ENV_MASTER
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "api.nuviot.com",
            };

            _appConfig.DeviceRepoId = "7D9871D47B7F4BDCB338FAE4C1CBF947";
#endif

            ResourceSupport.UseCustomColors = true;
            ResourceSupport.UseCustomfonts = true;

            this.RegisterStyle(new AppStyle());
            

            var clientAppInfo = new ClientAppInfo();            
            SLWIOC.RegisterSingleton<IClientAppInfo>(clientAppInfo);            
            SLWIOC.RegisterSingleton<IAppConfig>(_appConfig);

            SLWIOC.Register<IDeviceManagementClient, DeviceManagementClient>();

            var navigation = new ViewModelNavigation(this);
            SLWIOC.RegisterSingleton<IViewModelNavigation>(navigation);
            LagoVista.XPlat.Core.Startup.Init(this, navigation);
            LagoVista.Client.Core.Startup.Init(serverInfo);

            navigation.Add<MainViewModel, Views.MainView>();
            
            navigation.Add<SettingsViewModel, Views.SettingsView>();

            navigation.Add<GeoFencesViewModel, Views.GeoFencesView>();
            navigation.Add<GeoFenceViewModel, Views.GeoFenceView>();

            navigation.Add<ConfigureAlertsViewModel, Views.ConfigureAlertsView>();
            navigation.Add<ConfigureAlertViewModel, Views.ConfigureAlertView>();

            navigation.Add<ComponentViewModel, Views.ComponentView>();
            navigation.Add<ConfigurationViewModel, Views.ConfigurationView>();
            navigation.Add<LiveDataViewModel, Views.LiveDataView>();
          
            navigation.Add<SplashViewModel, Views.SplashView>();
           
            navigation.Start<SplashViewModel>();

            var dmClient = SLWIOC.Create<IDeviceManagementClient>();

            SLWIOC.RegisterSingleton<IDeviceManagementClient>(dmClient);
        }

        public void HandleURIActivation(Uri uri)
        {
            var logger = SLWIOC.Get<ILogger>();
            if (this.MainPage == null)
            {
                logger.AddCustomEvent(LogLevel.Error, "App_HandleURIActivation", "Main Page Null");
            }
            else
            {
                var page = this.MainPage as LagoVistaNavigationPage;
                if (page != null)
                {
                    page.HandleURIActivation(uri);
                }
                else
                {

                    logger.AddCustomEvent(LogLevel.Error, "App_HandleURIActivation", "InvalidPageType - Not LagoVistaNavigationPage", new System.Collections.Generic.KeyValuePair<string, string>("type", this.MainPage.GetType().Name));
                }
            }
        }

        public void SetVersionInfo(VersionInfo versionInfo)
        {
            _appConfig.Version = versionInfo;
        }


        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static string ResolveBTDeviceIdKey(String repoId, String deviceId)
        {
            return $"BT_DEVICE_ID_{repoId}_{deviceId}";
        }
    }
}
