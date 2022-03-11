using BugLog;
using BugLog.Managers;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BugLog.UWP
{
    public class ProcessRunner : IProcessRunner
    {
        ConsoleWriter _consoleWriter;
        public ProcessRunner()
        {
        }

        public void Init(ConsoleWriter consoleWriter)
        {
            _consoleWriter = consoleWriter;
        }

        public async Task RunProcess(string path, string cmd, string args, string actionType, bool clearConsole = true, bool checkRemote = true)
        {
            await Task.Run(() =>
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = cmd,
                        Arguments = args,
                        UseShellExecute = false,
                        WorkingDirectory = path,                        
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,                         
                    }
                };

                _consoleWriter.Clear();
                _consoleWriter.AddMessage(LogType.Message, $"cd {path}");
                _consoleWriter.AddMessage(LogType.Message, $"{proc.StartInfo.FileName} {proc.StartInfo.Arguments}");

                proc.Start();

                while (!proc.StandardOutput.EndOfStream)
                {
                    var line = proc.StandardOutput.ReadLine().Trim();
                    _consoleWriter.AddMessage(LogType.Message, line);
                }

                while (!proc.StandardError.EndOfStream)
                {
                    var line = proc.StandardError.ReadLine().Trim();
                    _consoleWriter.AddMessage(LogType.Error, line);
                }

                if (proc.ExitCode == 0)
                {
                    _consoleWriter.AddMessage(LogType.Success, $"Success {actionType}");
                }
                else
                {
                    _consoleWriter.AddMessage(LogType.Error, $"Error {actionType}!");
                }

                _consoleWriter.AddMessage(LogType.Message, "------------------------------");
                _consoleWriter.AddMessage(LogType.Message, "");
                _consoleWriter.Flush();
            });            
        }
    }
}
