using LagoVista.Client.Core;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using System;

namespace LagoVista.AppLoader
{
    public class ClientAppInfo : IClientAppInfo
    {
        public Type MainViewModel => typeof(DFUViewModel);
    }
}
