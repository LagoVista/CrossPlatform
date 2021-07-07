using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LagoVista.PlatformManager
{
    public class AppConfig : IAppConfig
    {
        public AppConfig()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android: PlatformType = PlatformTypes.Android; break;
                case Device.iOS: PlatformType = PlatformTypes.iPhone; break;
                case Device.UWP: PlatformType = PlatformTypes.WindowsUWP; break;
            }
        }

        public PlatformTypes PlatformType { get; private set; }

        public Environments Environment { get; set; }

        public string WebAddress { get; set; }

        public string AppName => "IoT Platform Manager";

        public string AppLogo => "appicon.png";

        public string CompanyLogo => "slsys.png";

        public bool EmitTestingCode => true;

        public string AppId => "0200AC799F384D15B1D233E73793D416";
        public string ClientType => "mobileapp";

        public VersionInfo Version { get; set; }

        public string CompanyName => "Software Logistics, LLC";

        public string CompanySiteLink => "https://support.nuviot.com/help.html#/information/company";

        public string AppDescription => "IoT Platform Manager is a free utility provided by Software Logistics, LLC to help monitor and manage IoT Server applications built with IoT App Studio.";

        public string TermsAndConditionsLink => "https://app.termly.io/document/terms-of-use-for-saas/90eaf71a-610a-435e-95b1-c94b808f8aca";

        public string PrivacyStatementLink => "https://app.termly.io/document/privacy-policy-for-website/f0b67cde-2a08-4fe8-a35e-5e4571545d00";

        public AuthTypes AuthType => AuthTypes.User;

        public EntityHeader SystemOwnerOrg => null;

        public string APIToken => String.Empty;

        public string InstanceId { get; set; }
        public string InstanceAuthKey { get; set; }
        public string DeviceId { get; set; }
        public string DeviceRepoId { get; set; }
    }
}
