using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Client.Core.Models
{    
    public class ConsoleOutput
    {
        public string Output { get; set; }
        public LogType LogType { get; set; }

        public override string ToString()
        {
            return Output;
        }
    }
}

