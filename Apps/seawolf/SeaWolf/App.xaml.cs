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

#if ENV_DEV
        public static string AppCenterId_iOS = "5beabb46-53d2-4c25-9c1f-e21dff576eef";
#elif ENV_MASTER
        public static string AppCenterId_iOS = "6e44a482-46b8-443e-bf96-1ad2f196f318";
#endif
        public static App Instance { get; private set; }

        public App()
        {
            Device.SetFlags(new string[] { "Shapes_Experimental" });
            InitializeComponent();

            App.Instance = this;

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

            _appConfig.SystemOwnerOrg = new EntityHeader()
            {
                Id = "FD36FAE278C343649B28F29A51867720",
                Text = "Seawolf Dev"
            };

            _appConfig.AppId = "BA98528EBC334DA1AFFAF8370ACB6035";
            _appConfig.InstanceId = "3CFFB06D07F04542BFE3327727CF07DB";
            _appConfig.DeviceRepoId = "61CF8C6E1688479DB252794D9ABC983A";

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

            _appConfig.SystemOwnerOrg = new EntityHeader()
            {
                Id = "B20031613DFB4AF89F4FE8EE25AF7FFE",
                Text = "SeaWolf Marine"
            };

            _appConfig.AppId = "6819D571A0A84371BB236BCB4219D1D0";
            _appConfig.InstanceId = "68AD559A50644D5F92087E429E57D947";
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
