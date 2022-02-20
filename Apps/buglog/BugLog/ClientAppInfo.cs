using LagoVista.Client.Core;
using BugLog.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugLog
{
    public class ClientAppInfo : IClientAppInfo
    {
        public Type MainViewModel => typeof(MainViewModel);
    }
}
