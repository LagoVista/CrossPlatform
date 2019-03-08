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
                Version = new VersionInfo()
                {
                    Major = 1,
                    Minor = 2,
                    Build = 218,
                    Revision = 1130
                };
            }

            public PlatformTypes PlatformType => PlatformTypes.WindowsUWP;

            public Environments Environment => Environments.Local;

            public string WebAddress => "http://localhost:5000";

            public string AppName => "The Sample App";

            public string AppLogo { get { return "applogo.png"; } }
            public string APIToken => "";

            public string CompanyLogo { get { return "slsys.png"; } }

            public bool EmitTestingCode => true;

            public string AppId => "C2781A0A72DB4634975F868F0C0405C3";

            public string ClientType => "mobileapp";

            public VersionInfo Version { get; private set; }

            public string CompanyName { get { return "Software Logistics"; } }

            public string CompanySiteLink { get { return "https://www.slsys.net"; } }

            public string AppDescription { get { return "This app is pretty cool, you should really look to doing something with it a little bit of a longer content just to see what happens when it breaks.\r\nI'm writing a few lines here...this is what I think of this...hellow\r\nMore lines"; } }

            public string TermsAndConditionsLink { get { return "https://www.nuviot.com"; } }

            public string PrivacyStatementLink { get { return "https://www.nuviot.com"; } }

            public AuthTypes AuthType { get; set; } = AuthTypes.User;

            public string InstanceId { get; set; }
            public string InstanceAuthKey { get; set; }
            public string DeviceId { get; set; }
            public string DeviceRepoId { get; set; }
        }

    }
}
