using LagoVista.Client.Core;
using SeaWolf.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeaWolf
{
    public class ClientAppInfo : IClientAppInfo
    {
        public Type MainViewModel => typeof(MainViewModel);
    }
}
