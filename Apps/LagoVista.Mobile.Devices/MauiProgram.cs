using LagoVista.Client.Core;
using LagoVista.Client.Core.Auth;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.Mobile.Devices.ViewModels;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.XPlat.Maui;
using LagoVista.XPlat.Maui.Pages;
using LagoVista.XPlat.Maui.Services;
using Microsoft.Extensions.Logging;

namespace LagoVista.Mobile.Devices
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

         
            var serverInfo = new ServerInfo()
            {
                RootUrl = "www.nuviot.com",
                Port = 443,
                SSL = true,
            };
    
            var nav = new ViewModelNavigation();
            nav.RegisterView<SplashViewModel, SplashPage>();
            nav.RegisterView<LoginViewModel, LoginPage>();
            nav.RegisterView<AboutViewModel, AboutPage>();
            nav.RegisterView<MainViewModel, MainPage>();
            SLWIOC.RegisterSingleton<IAppConfig, AppConfig>();
            SLWIOC.RegisterSingleton<LagoVista.Core.PlatformSupport.IDeviceInfo, AppDeviceInfo>();
            SLWIOC.RegisterSingleton<IClientAppInfo, DeviceAppInfo>();

            SLWIOC.RegisterSingleton<IViewModelNavigation>(nav);

            LagoVista.XPlat.Maui.Startup.Init();
            LagoVista.Client.Core.Startup.Init(serverInfo);
            
            builder.Services.AddSingleton<LagoVista.Core.PlatformSupport.IDeviceInfo, AppDeviceInfo>();
            builder.Services.AddSingleton<IViewModelNavigation>(nav);
            builder.Services.AddSingleton<IClientAppInfo, DeviceAppInfo>();
            builder.Services.AddSingleton<IAuthClient, AuthClient>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<SplashViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    public class AppDeviceInfo : LagoVista.Core.PlatformSupport.IDeviceInfo
    {
        public string DeviceUniqueId => "1234567890";
        public string DeviceType => "Mobile";
    }

    public class DeviceAppInfo : IClientAppInfo
    {
        public Type MainViewModel => typeof(MainViewModel);
    }

    public class AppConfig : IAppConfig
    {
        public PlatformTypes PlatformType => PlatformTypes.WindowsUWP;

        public Environments Environment => Environments.Production;

        public AuthTypes AuthType => AuthTypes.User;

        public EntityHeader SystemOwnerOrg => throw new NotImplementedException();

        public string WebAddress => throw new NotImplementedException();

        public string CompanyName => throw new NotImplementedException();

        public string CompanySiteLink => throw new NotImplementedException();

        public string AppName => throw new NotImplementedException();

        public string AppId => throw new NotImplementedException();

        public string APIToken => throw new NotImplementedException();

        public string AppDescription => throw new NotImplementedException();

        public string TermsAndConditionsLink => throw new NotImplementedException();

        public string PrivacyStatementLink => throw new NotImplementedException();

        public string ClientType => throw new NotImplementedException();

        public string AppLogo => throw new NotImplementedException();

        public string CompanyLogo => throw new NotImplementedException();

        public string InstanceId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string InstanceAuthKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string DeviceId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string DeviceRepoId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string DefaultDeviceLabel => throw new NotImplementedException();

        public string DefaultDeviceLabelPlural => throw new NotImplementedException();

        public bool EmitTestingCode => throw new NotImplementedException();

        public VersionInfo Version => throw new NotImplementedException();

        public string AnalyticsKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
