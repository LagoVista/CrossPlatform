using LagoVista.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System;
using Xamarin.Forms;

namespace SeaWolf.Models
{
    public class SensorSummary : ModelBase
    {
        public PortConfig Config { get; set; }
        public AppSpecificSensorTypes SensorType { get; set; }

        private string _value;
        public string Value
        {
            set
            {
                Set(ref _value, value);
                if (Convert.ToDouble(value) < Config.LowThreshold)
                {
                    SensorColor = Color.Blue;
                }
                else if (Convert.ToDouble(value) > Config.HighTheshold)
                {
                    SensorColor = Color.Red;
                }
                else 
                {
                    SensorColor = Color.Green;
                }
            }
            get => _value;
        }

        Color _sensorColor;
        public Color SensorColor
        {
            set { Set(ref _sensorColor, value); }
            get => _sensorColor;
        }
    }
}
