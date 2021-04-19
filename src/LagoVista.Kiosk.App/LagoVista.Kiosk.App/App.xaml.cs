//#define ENV_LOCAL
#define ENV_DEV
//#define ENV_STAGE
//#define ENV_MASTER

using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LagoVista.Client.Core;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.XPlat.Core.Services;
using LagoVista.XPlat.Core.Views;
using LagoVista.Kiosk.App.ViewModels;
using LagoVista.Kiosk.App.Views;
using System.Diagnostics;

namespace LagoVista.Kiosk.App
{
	public partial class App : Xamarin.Forms.Application
    {

		public static App Instance { get; private set; }

		public App()
		{
			InitializeComponent();
			App.Instance = this;
			InitServices();
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
				if (this.MainPage is LagoVistaNavigationPage page)
				{
					page.HandleURIActivation(uri);
				}
				else
				{

					logger.AddCustomEvent(LogLevel.Error, "App_HandleURIActivation", "InvalidPageType - Not LagoVistaNavigationPage", new System.Collections.Generic.KeyValuePair<string, string>("type", this.MainPage.GetType().Name));
				}
			}
		}

		public class ClientAppInfo : IClientAppInfo
		{
			public Type MainViewModel { get; set; }
        }

        private void InitServices()
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

            var clientAppInfo = new ClientAppInfo()
            {
                MainViewModel = typeof(HomeViewModel)
            };

            //var dark = new LagoVista.Colors.ThemeDark();
            //Resources.MergedDictionaries.Add(dark);

            //Device.SetFlags(new string[] { "AppTheme_Experimental" });
            App.Current.RequestedThemeChanged += Current_RequestedThemeChanged;
            Debug.WriteLine("APP THEME: " + App.Current.UserAppTheme);
            DeviceInfo.Register();

            var appConfig = new AppConfig();
            //appConfig.AuthType = AuthTypes.DeviceUser;
            appConfig.DeviceRepoId = "189D6E2F61F444529AF881159F6C2190";

            SLWIOC.RegisterSingleton<IAppConfig>(appConfig);
            SLWIOC.RegisterSingleton<IClientAppInfo>(clientAppInfo);

            var navigation = new ViewModelNavigation(this);
            navigation.Add<HomeViewModel, HomeView>();
            navigation.Add<KioskViewerViewModel, KioskViewerView>();
            navigation.Add<SplashViewModel, SplashView>();

            SLWIOC.RegisterSingleton<IViewModelNavigation>(navigation);
            LagoVista.Client.Core.Startup.Init(serverInfo);

            try
            {
                LagoVista.XPlat.Core.Startup.Init(this, navigation);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            navigation.Start<SplashViewModel>();
        }

        private void Current_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            Debug.WriteLine("APP THEME CHANGED: " + e.RequestedTheme);
        }

        //protected override void OnStart()
        //{
        //}

        //protected override void OnSleep()
        //{
        //}

        //protected override void OnResume()
        //{
        //}
    }
}