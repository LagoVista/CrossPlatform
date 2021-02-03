using LagoVista.Client.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace LagoVista.XPlat.WPF.Services
{
    public class ProcessOutputWriter : LagoVista.Client.Core.Interfaces.IProcessOutputeWriter
    {
        ObservableCollection<ConsoleOutput> _buffer = new ObservableCollection<ConsoleOutput>();

        public ProcessOutputWriter()
        {
            Output = new ObservableCollection<ConsoleOutput>();
        }

        public void AddMessage(LogType type, String message)
        {
            lock (_buffer)
            {
                _buffer.Add(new ConsoleOutput()
                {
                    LogType = type,
                    Output = message
                });
            }
        }

        public void Flush(bool clear = false)
        {
            Collection<ConsoleOutput> tmpBuffer = new Collection<ConsoleOutput>();
            lock (_buffer)
            {
                foreach (var item in _buffer)
                {
                    tmpBuffer.Add(item);
                }
                _buffer.Clear();
            }

            Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate
            {
                lock (Output)
                {
                    if (clear)
                    {
                        Output.Clear();
                    }

                    foreach (var msg in tmpBuffer)
                    {
                        Output.Add(msg);
                    }
                }
            });
        }

        public ObservableCollection<ConsoleOutput> Output
        {
            get;
        }
    }
}
