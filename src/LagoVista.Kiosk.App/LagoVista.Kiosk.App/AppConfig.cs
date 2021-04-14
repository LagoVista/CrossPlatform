using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;

namespace LagoVista.Kiosk.App
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
					Minor = 0,
					Build = 0,
					Revision = 0
				};
            }

            public PlatformTypes PlatformType => PlatformTypes.WindowsUWP;

            public Environments Environment => Environments.Local;

            public string WebAddress => "http://localhost:5000";

            public string AppName => "NuvIoT Kiosk Viewer";

            public string AppLogo { get { return "nuviot.png"; } }
            public string APIToken => "";

            public string CompanyLogo { get { return "slsys.png"; } }

            public bool EmitTestingCode => true;

            public string AppId => "CA2F8BF81F3B43659027C38EFEED9DE9";

            public string ClientType => "mobileapp";

            public VersionInfo Version { get; private set; }

            public string CompanyName { get { return "Software Logistics"; } }

            public string CompanySiteLink { get { return "https://www.slsys.net"; } }

            public string AppDescription { get { return "A viewer of the pre-configured kiosks available for your user via your organization, powered by NuvIoT."; } }

            public string TermsAndConditionsLink { get { return "https://www.nuviot.com"; } }

            public string PrivacyStatementLink { get { return "https://www.nuviot.com"; } }

            public AuthTypes AuthType { get; set; } = AuthTypes.User;

            public string InstanceId { get; set; }
            public string InstanceAuthKey { get; set; }
            public string DeviceId { get; set; }
            public string DeviceRepoId { get; set; }

            public EntityHeader SystemOwnerOrg { get; set; }
        }
	}
}
