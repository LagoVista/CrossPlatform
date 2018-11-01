using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Client.Devices.Models
{
    public class DataStreamResult : Dictionary<string, object>
    {
        public DataStreamResult()
        {
            //            Fields = new Dictionary<string, object>();
        }

        public string Timestamp { get; set; }
        //public Dictionary<string, object> Fields { get; set; }

    }
}
