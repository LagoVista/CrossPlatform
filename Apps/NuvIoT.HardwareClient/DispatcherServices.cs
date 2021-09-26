using System;
using System.Windows.Threading;

namespace LagoVista.Core.UWP.Services
{
    public class DispatcherServices : IDispatcherServices
    {
        Dispatcher _dispatcher;
        public DispatcherServices(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Invoke(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }
    }
}
