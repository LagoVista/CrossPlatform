using LagoVista.Core;
using System;

namespace LagoVista.Core.UWP.Services
{
    public class DispatcherServices : IDispatcherServices
    {
        Windows.UI.Core.CoreDispatcher _dispatcher;

        private DispatcherServices() { }

        public DispatcherServices(Windows.UI.Core.CoreDispatcher  dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async void Invoke(Action action)
        {           
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => action());
        }
    }
}
