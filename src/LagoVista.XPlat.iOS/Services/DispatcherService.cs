using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace LagoVista.XPlat.iOS.Services
{
    public class DispatcherService : IDispatcherServices
    {
        UIApplication _app;

        public DispatcherService(UIApplication app)
        {
            _app = app;
        }

        public void Invoke(Action action)
        {
            action();
        }
    }
}