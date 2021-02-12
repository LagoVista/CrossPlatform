using System;
using LagoVista.Core;
using Android.Content;

namespace LagoVista.XPlat.Droid.Services
{
    public class DispatcherServices : IDispatcherServices
    {
        Context _context;
        public DispatcherServices(Context context)
        {
            _context = context;
        }

        public void Invoke(Action action)
        {
            var context = global::Android.App.Application.Context;
            action();
            
        }
    }
}