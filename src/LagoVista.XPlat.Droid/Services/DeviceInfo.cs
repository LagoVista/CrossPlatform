using LagoVista.Core.PlatformSupport;
using static Android.Provider.Settings;

namespace LagoVista.XPlat.Droid.Services
{
    public class DeviceInfo : IDeviceInfo
    {
        public string DeviceType
        {
            get{return "Android";}
        }

        public string DeviceUniqueId
        {
            get
            {
                var context = global::Android.App.Application.Context;
                return Secure.GetString(context.ContentResolver, Secure.AndroidId); 
            }
        }
    }
}