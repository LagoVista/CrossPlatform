#define ENV_MASTER
using LagoVista.Client.Core;
using LagoVista.Client.Core.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.WPF.PlatformSupport;
using LagoVista.XPlat.WPF.Services;
using System.Windows;

namespace BugLog.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var appConfig = new AppConfig();


            DeviceInfo.Register("ABC1234");
            WPFDeviceServices.Init(this.Dispatcher);
            SLWIOC.Register<IAppConfig>(appConfig);
            SLWIOC.Register<IClientAppInfo>(new ClientAppInfo());

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

  /*          _appConfig.SystemOwnerOrg = new EntityHeader()
            {
                Id = "FD36FAE278C343649B28F29A51867720",
                Text = "BugLog Dev"
            };
*/
            _appConfig.AppId = "6D2620D8968D4157BA501F9DACF5F55A";
//            _appConfig.InstanceId = "3CFFB06D07F04542BFE3327727CF07DB";
  //          _appConfig.DeviceRepoId = "61CF8C6E1688479DB252794D9ABC983A";

#elif ENV_LOCALDEV
            var serverInfo = new ServerInfo()
            {
                SSL = false,
                RootUrl = "localhost:5001",
            };

           /* _appConfig.SystemOwnerOrg = new EntityHeader()
            {
                Id = "FD36FAE278C343649B28F29A51867720",
                Text = "BugLog Dev"
            };*/

            _appConfig.AppId = "6D2620D8968D4157BA501F9DACF5F55A";
//          _appConfig.InstanceId = "3CFFB06D07F04542BFE3327727CF07DB";
//          _appConfig.DeviceRepoId = "61CF8C6E1688479DB252794D9ABC983A";

            //   make sure it doesn't compile....add in a device repo id
#elif ENV_MASTER
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "api.nuviot.com",
            };

            appConfig.SystemOwnerOrg = new EntityHeader()
            {
                Id = "AA2C78499D0140A5A9CE4B7581EF9691",
                Text = "Software Logistics"
            };

            appConfig.AppId = "63E32888336348D1A0B26DC4BBD88208";
#endif


            LagoVista.Client.Core.Startup.Init(serverInfo);
        }
    }
}
