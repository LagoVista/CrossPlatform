using LagoVista.Client.Core.Models;
using LagoVista.Core;
using System.Collections.ObjectModel;

namespace LagoVista.Client.Core
{
    public enum LogType
    {
        Message,
        Warning,
        Error,
        Success,
    }

    public interface IConsoleWriter
    {
        void Init(ObservableCollection<ConsoleOutput> output, IDispatcherServices dispatcher);

        void AddMessage(LogType type, string message, bool autoFlush = false);
        void Clear();
        void Flush(bool clear = false);
    }
}