using LagoVista.Core.PlatformSupport;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Push;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;

namespace LagoVista.Core.UWP.Loggers
{
    public class AppCenterLogger : ILogger
    {
        public enum AppCenterModes
        {
            Crashes,
            Analytics,
            Push,
        }

        KeyValuePair<String, String>[] _args;

        public bool DebugMode { get; set; }

        public AppCenterLogger(string key, params AppCenterModes[] args)
        {
            var types = new List<Type>();
            foreach(var arg in args)
            {
                switch(arg)
                {
                    case AppCenterModes.Analytics:
                        types.Add(typeof(Analytics));
                        break;
                    case AppCenterModes.Crashes:
                        types.Add(typeof(Crashes));
                        break;
                    case AppCenterModes.Push:
                        types.Add(typeof(Push));
                        break;
                }
            }

            AppCenter.Start($"uwp={key}", types.ToArray());
        }


        public TimedEvent StartTimedEvent(string area, string description)
        {
            return new TimedEvent(area, description);
        }

        public void EndTimedEvent(TimedEvent evt)
        {
            var duration = DateTime.Now - evt.StartTime;

            AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Message, evt.Area, evt.Description, new KeyValuePair<string, string>("duration", Math.Round(duration.TotalSeconds, 4).ToString()));
        }

        public void AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel level, string area, string message, params KeyValuePair<string, string>[] args)
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("Area", area);
            dictionary.Add("Level", level.ToString());

            if (_args != null)
            {
                foreach (var arg in _args)
                {
                    dictionary.Add(arg.Key, arg.Value);
                }
            }

            foreach (var arg in args)
            {
                dictionary.Add(arg.Key, arg.Value);
            }

            Analytics.TrackEvent(message, dictionary);
        }

        public void AddException(string area, Exception ex, params KeyValuePair<string, string>[] args)
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("Area", area);
            dictionary.Add("Type", "exception");
            dictionary.Add("StackTrace", ex.StackTrace.Substring(0));

            if (_args != null)
            {
                foreach (var arg in _args)
                {
                    dictionary.Add(arg.Key, arg.Value);
                }
            }

            foreach (var arg in args)
            {
                dictionary.Add(arg.Key, arg.Value);
            }


            Analytics.TrackEvent(ex.Message, dictionary);
        }

        public void AddKVPs(params KeyValuePair<String, String>[] args)
        {
            _args = args;
        }

        public void TrackEvent(string message, Dictionary<string, string> args)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                dictionary.Add(arg.Key, arg.Value);
            }

            if (_args != null)
            {
                foreach (var arg in _args)
                {
                    dictionary.Add(arg.Key, arg.Value);
                }
            }

            Analytics.TrackEvent(message, dictionary);
        }
    }
}
