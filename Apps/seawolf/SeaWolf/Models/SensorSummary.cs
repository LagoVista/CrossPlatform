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
                var dblValue = Convert.ToDouble(value);
                var range = Config.HighTheshold - Config.LowThreshold;
                var warningThreshold = range * 0.20;

                Set(ref _value, value);
                if (dblValue < Config.LowThreshold ||
                    dblValue > Config.HighTheshold)
                {
                    SensorBackgroundColor = Color.FromRgb(0xE9, 0x5C, 0x5D);
                    SensorForegroundColor = Color.White;
                    OutOfTolerance = true;
                    Warning = false;
                }
                else if (dblValue < (Config.LowThreshold + warningThreshold) ||
                         dblValue > Config.HighTheshold - warningThreshold)

                {
                    SensorBackgroundColor = Color.FromRgb(0xFF, 0xC8, 0x7F);
                    SensorForegroundColor = Color.White;
                    OutOfTolerance = false;
                    Warning = true;
                }
                else 
                {
                    SensorBackgroundColor = Color.FromRgb(0x55, 0xA9, 0xF2);
                    SensorForegroundColor = Color.FromRgb(0x21, 0x21, 0x21);
                    OutOfTolerance = false;
                    Warning = false;
                }
            }
            get => _value;
        }

        Color _sensorBackgroundColor;
        public Color SensorBackgroundColor
        {
            set { Set(ref _sensorBackgroundColor, value); }
            get => _sensorBackgroundColor;
        }

        Color _sensorForegroundColor;
        public Color SensorForegroundColor
        {
            set { Set(ref _sensorForegroundColor, value); }
            get => _sensorForegroundColor;
        }

        bool _outOfTolerance;
        public bool OutOfTolerance
        {
            get => _outOfTolerance;
            set => Set(ref _outOfTolerance, value);
        }

        bool _warning;
        public bool Warning
        {
            get => _warning;
            set => Set(ref _warning, value);
        }
    }
}
