using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BugLog.Managers
{
    public class ConsoleWriter 
    {
        private readonly ObservableCollection<ConsoleOutput> _buffer = new ObservableCollection<ConsoleOutput>();
        private readonly ObservableCollection<ConsoleOutput> _output;
        private readonly IDispatcherServices _dispatcher;

        public ConsoleWriter(ObservableCollection<ConsoleOutput> output, IDispatcherServices dispatcher)
        {
            _output = output;
            _dispatcher = dispatcher;
        }

        public void AddMessage(LogType type, String message, bool autoFlush = false)
        {
            lock (_buffer)
            {
                _buffer.Add(new ConsoleOutput()
                {
                    LogType = type,
                    Output = message
                });
            }

            if (autoFlush)
            {
                Flush();
            }
        }

        public void Flush(bool clear = false)
        {
            var tmpBuffer = new Collection<ConsoleOutput>();
            lock (_buffer)
            {
                foreach (var item in _buffer)
                {
                    tmpBuffer.Add(item);
                }
                _buffer.Clear();
            }

            _dispatcher.Invoke((Action)delegate
            {
                lock (_output)
                {
                    if (clear)
                    {
                        _output.Clear();
                    }

                    foreach (var msg in tmpBuffer)
                    {
                        _output.Add(msg);
                    }
                }
            });
        }

        public void Clear()
        {
            _dispatcher.Invoke((Action)delegate
            {
                lock (_output)
                {
                        _output.Clear();               
                }
            });
        }
    }

    public enum LogType
    {
        Message,
        Warning,
        Error,
        Success,
    }

    public class ConsoleOutput
    {
        public string Output { get; set; }
        public LogType LogType { get; set; }
    }
}
