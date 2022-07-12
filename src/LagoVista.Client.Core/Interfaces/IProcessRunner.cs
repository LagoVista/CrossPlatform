using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core
{
    public interface IProcessRunner
    {
        void Init(IConsoleWriter consoleWriter);
        Task RunProcess(string path, string cmd, string args, string actionType, bool clearConsole = true, bool checkRemote = true);
    }
}
