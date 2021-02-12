using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.ObjectModel;

namespace LagoVista.XPlat.Droid.Services
{
    public class ProcessWriter : IProcessOutputWriter
    {
        public ObservableCollection<ConsoleOutput> Output => new ObservableCollection<ConsoleOutput>();

        private readonly Android.Content.Context _context;

        public ProcessWriter(Android.Content.Context context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void AddMessage(LogType type, string message)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                Output.Add(new ConsoleOutput()
                {
                    LogType = type,
                    Output = message,
                });
            });
        }

        public void Flush(bool clear = false)
        {
            Output.Clear();
        }
    }
}