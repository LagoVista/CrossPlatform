using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace LagoVista.Core.UWP.Services
{
    public class Timer : ITimer
    {
        ThreadPoolTimer _nativeTimer;

        public TimeSpan Interval { get; set; }

        public event EventHandler Tick;

        public Object State { get; set; }

        public bool InvokeOnUIThread { get; set; }

        public void Dispose()
        {
            lock(this)
            {
                if (_nativeTimer != null)
                {
                    _nativeTimer.Cancel();
                    _nativeTimer = null;
                }
            }
        }

        private void TickHandler(Object obj)
        {
            if (Tick != null)
            {
                if (InvokeOnUIThread)
                {
                    Core.PlatformSupport.Services.DispatcherServices.Invoke(() =>
                    {
                        Tick(this, null);
                    });
                }
                else
                    Tick(this, null);
            }
        }

        public void Start()
        {
            _nativeTimer = ThreadPoolTimer.CreatePeriodicTimer(TickHandler, Interval);            
        }

        public void Stop()
        {
            lock(this)
            {
                if (_nativeTimer != null)
                {
                    _nativeTimer.Cancel();
                    _nativeTimer = null;
                }
            }
        }
    }

    public class TimerFactory : ITimerFactory
    {
        public ITimer Create(TimeSpan interval)
        {
            return new Timer() { Interval = interval, InvokeOnUIThread = false };
        }
    }
}
