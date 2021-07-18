using LagoVista.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;

namespace SeaWolf.Models
{
    public class SensorSummary
    {
        public PortConfig Config { get; set; }
        public AppSpecificSensorTypes SensorType { get; set; }
        public string Value { get; set; }
    }
}
