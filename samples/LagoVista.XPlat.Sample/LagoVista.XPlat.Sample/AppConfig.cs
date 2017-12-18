using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;

namespace LagoVista.XPlat.Sample
{
    public partial class App
    {
        public class AppConfig : IAppConfig
        {
            public AppConfig()
            {
                Version = new VersionInfo();
            }

            public PlatformTypes PlatformType => PlatformTypes.WindowsUWP;

            public Environments Environment => Environments.Local;

            public string WebAddress => "http://localhost:5000";

            public string AppName => "Remote Simulator";

            public string AppLogo => "";

            public string CompanyLogo => "";

            public bool EmitTestingCode => true;

            public string AppId => "C2781A0A72DB4634975F868F0C0405C3";
            public string ClientType => "mobileapp";

            public VersionInfo Version { get; private set; }
        }

    }
}
