using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.System.Profile;

namespace LagoVista.Core.UWP.Services
{
    public class DeviceInfo : IDeviceInfo
    {
        public string DeviceType
        {
            get { return Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily; }
        }

        public string DeviceUniqueId
        {
            get
            {
                var hardwareId = HardwareIdentification.GetPackageSpecificToken(null).Id;
                var hasher = HashAlgorithmProvider.OpenAlgorithm("MD5");
                var hashed = hasher.HashData(hardwareId);

                return CryptographicBuffer.EncodeToHexString(hashed);
            }
        }
    }
} 