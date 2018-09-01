using LagoVista.Client.Core;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using LagoVista.XPlat.Core.Services;
using LagoVista.XPlat.Core.Views.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace LagoVista.XPlat.Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            InitServices();
        }

        public class ClientAppInfo : IClientAppInfo
        {
            public Type MainViewModel { get; set; }
        }

        private void InitServices()
        {
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "dev-api.nuviot.com",
            };

            var clientAppInfo = new ClientAppInfo()
            {
                MainViewModel = typeof(MainViewModel)
            };

            DeviceInfo.Register();

            SLWIOC.RegisterSingleton<IAppConfig>(new AppConfig());
            LagoVista.Client.Core.Startup.Init(serverInfo);
            SLWIOC.RegisterSingleton<IClientAppInfo>(clientAppInfo);
            
            var navigation = new ViewModelNavigation(this);
            navigation.Add<MainViewModel, MainPage>();
            navigation.Add<ServicesViewModel, ServicesView>();
            navigation.Add<SecureStorageViewModel, SecureStorageView>();
            navigation.Add<ControlSampleViewModel, ControlSampleView>();
            navigation.Add<ViewModel2, Model2View>();
            navigation.Add<FullPageViewModel, FullScreenPage>();



            SLWIOC.RegisterSingleton<IViewModelNavigation>(navigation);

            try
            {
                LagoVista.XPlat.Core.Startup.Init(this, navigation);
            
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            navigation.Start<MainViewModel>();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

    }
}
