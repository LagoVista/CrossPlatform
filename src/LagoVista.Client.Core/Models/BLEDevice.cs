using LagoVista.Core.Models;
using System;
using System.Collections.ObjectModel;

namespace LagoVista.Client.Core.Models
{
    public class BLEDevice : ModelBase
    {
        public string DeviceName { get; set; }
        public string DeviceAddress { get; set; }

        public bool _connected;
        public bool Connected 
        {
            get => _connected;
            set => Set(ref _connected, value);
        }

        private DateTime _lastSeen;
        public DateTime LastSeen
        {
            get => _lastSeen;
            set => Set(ref _lastSeen, value);
        }

        public ObservableCollection<BLEService> Services { get; } = new ObservableCollection<BLEService>();

        public ObservableCollection<BLECharacteristic> AllCharacteristics { get; } = new ObservableCollection<BLECharacteristic>();
    }
}
