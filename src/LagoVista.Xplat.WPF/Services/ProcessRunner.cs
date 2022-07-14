﻿using LagoVista.Client.Core;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.WPF.NetStd.Core.Services
{
    internal class ProcessRunner : IProcessRunner
    {
        IConsoleWriter _consoleWriter;
        public ProcessRunner()
        {
        }

        public void Init(IConsoleWriter consoleWriter)
        {
            _consoleWriter = consoleWriter;
        }

        public async Task<string> RunProcess(string path, string cmd, string args, string actionType, bool clearConsole = true, bool checkRemote = true)
        {
            var bldr = new StringBuilder();

            await Task.Run(() =>
            {
                try
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
                        bldr.AppendLine(line);
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
                }
                catch(Exception ex)
                {
                    _consoleWriter.AddMessage(LogType.Error, ex.Message);
                }
            });


            return bldr.ToString();
        }
    }
}