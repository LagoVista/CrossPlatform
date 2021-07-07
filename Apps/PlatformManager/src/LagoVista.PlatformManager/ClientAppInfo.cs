using LagoVista.Client.Core;
using LagoVista.PlatformManager.Core.ViewModels;
using System;

namespace LagoVista.PlatformManager
{
    public class ClientAppInfo : IClientAppInfo
    {
        public Type MainViewModel => typeof(MainViewModel);
    }
}
