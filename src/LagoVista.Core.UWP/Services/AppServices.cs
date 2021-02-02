using LagoVista.Client.Core.Interfaces;
using System;

namespace LagoVista.Core.UWP.Services
{
    public class AppServices : IAppServices
    {
        public string AppInstallDirectory => AppDomain.CurrentDomain.BaseDirectory;
    }
}
