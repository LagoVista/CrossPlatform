using System;
using Security;
using Foundation;
using LagoVista.Core.PlatformSupport;
using System.Diagnostics;

namespace LagoVista.XPlat.iOS.Services
{
    public class DeviceInfo : IDeviceInfo
    {
        public string DeviceType
        {
            get { return "iPhone"; }
        }

        public string DeviceUniqueId
        {
            get
            {
                var query = new SecRecord(SecKind.GenericPassword);
                query.Service = NSBundle.MainBundle.BundleIdentifier;
                query.Account = "UniqueID";

                var uniqueId = SecKeyChain.QueryAsData(query);
                if (uniqueId == null)
                {
                    query.ValueData = NSData.FromString(System.Guid.NewGuid().ToString());
                    var err = SecKeyChain.Add(query);
                    if (err != SecStatusCode.Success && err != SecStatusCode.DuplicateItem)
                    {
                        Debug.WriteLine(err.ToString());
                        //throw new Exception("Cannot store Unique ID");
                    }

                    return query.ValueData.ToString();
                }
                else
                {
                    return uniqueId.ToString();
                }
            }
        }
    }
}