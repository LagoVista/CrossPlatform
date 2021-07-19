using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SeaWolf
{


    public class AppConfig : IAppConfig
    {
        public const string BATTERY = "battery";
        public const string MOTION = "motion";
        public const string MOISTURE = "moisture";
        public const string HIGHWATERLEVEL = "highwaterlevel";
        public const string BATTERYSWITCH = "batteryswitch";
        public const string AMBIENTTEMP = "ambienttemperature";
        public const string WATERTEMP = "watertemperature";

        public AppConfig()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android: PlatformType = PlatformTypes.Android; break;
                case Device.iOS: PlatformType = PlatformTypes.iPhone; break;
                case Device.UWP: PlatformType = PlatformTypes.WindowsUWP; break;
            }

            SystemOwnerOrg = new EntityHeader()
            {
                Id = "B20031613DFB4AF89F4FE8EE25AF7FFE",
                Text = "SeaWolf Marine"
            };

            WebAddress = "https://www.NuvIoT.com";

            AppSpecificSensorTypes = new List<AppSpecificSensorTypes>()
            {
                new AppSpecificSensorTypes() { IconKey="fa-battery-full", Name="Battery", Key=BATTERY, Technology = SensorTechnology.ADC, SensorConfigId = 1, DefaultLowTolerance = 12, DefaultHighTolerance=15 },
                new AppSpecificSensorTypes() { IconKey="fa-warning", Name="Motion Detector", Key=MOTION, Technology = SensorTechnology.IO, SensorConfigId = 1, DefaultHighTolerance =1  },
                new AppSpecificSensorTypes() { IconKey="fa-tint", Name="Moisture", Key=MOISTURE, Technology = SensorTechnology.ADC, SensorConfigId = 1, DefaultLowTolerance=1 },
                new AppSpecificSensorTypes() { IconKey="fa-tint", Name="High Water Alarm", Key=HIGHWATERLEVEL, Technology = SensorTechnology.IO, SensorConfigId = 1, DefaultLowTolerance=1 },
                new AppSpecificSensorTypes() { IconKey="fa-power-off", Name="Battery Switch", Key=BATTERYSWITCH, Technology = SensorTechnology.ADC, SensorConfigId = 1, DefaultLowTolerance=12, DefaultHighTolerance=15 },
                new AppSpecificSensorTypes() { IconKey="fa-thermometer-full", Name="Ambient Temperature", Key=AMBIENTTEMP, Technology = SensorTechnology.IO, SensorConfigId = 6, DefaultLowTolerance=60, DefaultHighTolerance=90 },
                new AppSpecificSensorTypes() { IconKey="fa-thermometer-full", Name="Water Temperature", Key=WATERTEMP, Technology = SensorTechnology.IO, SensorConfigId = 4, DefaultLowTolerance=60, DefaultHighTolerance=90 },
            };
        }

        public PlatformTypes PlatformType { get; private set; }

        public Environments Environment { get; set; }

        public string WebAddress { get; set; } = "https://www.software-logistics.com";

        public string AppName => "SeaWolf Marine";

        public string AppLogo => "SeaWolf.png";

        public string CompanyLogo => "companylogo.png";
#if DEBUG
        public bool EmitTestingCode => true;
#else
        public bool EmitTestingCode => true;
#endif

        public string AppId => "6819D571A0A84371BB236BCB4219D1D0";
        public string ClientType => "mobileapp";

        public VersionInfo Version { get; set; }

        public string CompanyName => "Software Logistics, LLC";

        public string CompanySiteLink => "https://www.Software-Logistics.com";

        public string AppDescription => "SeaWolf is an application used to monitor you boats systems.";

        public string TermsAndConditionsLink => "https://app.termly.io/document/terms-of-use-for-saas/90eaf71a-610a-435e-95b1-c94b808f8aca";

        public string PrivacyStatementLink => "https://app.termly.io/document/privacy-policy-for-website/f0b67cde-2a08-4fe8-a35e-5e4571545d00";

        public AuthTypes AuthType => AuthTypes.User;

        public string APIToken => String.Empty;

        public string InstanceId { get; set; }
        public string InstanceAuthKey { get; set; }
        public string DeviceId { get; set; }
        public string DeviceRepoId { get; set; }

        public EntityHeader SystemOwnerOrg { get; set; }

        public IEnumerable<AppSpecificSensorTypes> AppSpecificSensorTypes { get; }
    }
}
