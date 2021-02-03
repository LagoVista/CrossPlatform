using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.WPF.Services
{
    public class Logger : ILogger
    {
        public bool DebugMode { get; set; }

        public void AddCustomEvent(LogLevel level, string tag, string customEvent, params KeyValuePair<string, string>[] args)
        {
        }

        public void AddException(string tag, Exception ex, params KeyValuePair<string, string>[] args)
        {
        }

        public void AddKVPs(params KeyValuePair<string, string>[] args)
        {
        }

        public void EndTimedEvent(TimedEvent evt)
        {
        }

        public TimedEvent StartTimedEvent(string area, string description)
        {
            return new TimedEvent(area, description);
        }

        public void TrackEvent(string message, Dictionary<string, string> parameters)
        {
        }
    }
}
