using BugLog.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BugLog
{
    public interface IProcessRunner
    {
        void Init(ConsoleWriter consoleWriter);
        Task RunProcess(string path, string cmd, string args, string actionType, bool clearConsole = true, bool checkRemote = true);
    }
}
