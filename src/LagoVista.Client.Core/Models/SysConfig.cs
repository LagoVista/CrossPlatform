using LagoVista.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Client.Core.Models
{
    public class SysConfig : ModelBase
    {
        bool _commissioned;
        [JsonProperty("commissioned")]
        public bool Commissioned
        {
            get => _commissioned;
            set => Set(ref _commissioned, value);
        }

        string _deviceId;
        [JsonProperty("deviceId")]
        public String DeviceId
        {
            get => _deviceId;
            set => Set(ref _deviceId, value);
        }

        string _wifiSSID;
        [JsonProperty("wifissid")]
        public String WiFiSSID
        {
            get => _wifiSSID;
            set => Set(ref _wifiSSID, value);
        }

        string _wifiPWD;
        [JsonProperty("wifipwd")]
        public String WiFiPWD
        {
            get => _wifiPWD;
            set => Set(ref _wifiPWD, value);
        }

        string _srvrHostName;
        [JsonProperty("srvrHostName")]
        public String SrvrHostName
        {
            get => _srvrHostName;
            set => Set(ref _srvrHostName, value);
        }

        bool _anonymous;
        [JsonProperty("anonymous")]
        public bool Anonymous
        {
            get => _anonymous;
            set => Set(ref _anonymous, value);
        }

        string _srvrUID;
        [JsonProperty("srvrUid")]
        public String SrvrUID
        {
            get => _srvrUID;
            set => Set(ref _srvrUID, value);
        }

        string _srvrPWD;
        [JsonProperty("srvrPwd")]
        public String SrvrPWD
        {
            get => _srvrPWD;
            set => Set(ref _srvrPWD, value);
        }

        bool _gpsEnabled;
        [JsonProperty("gpsEnabled")]
        public bool GPSEnabled
        {
            get => _gpsEnabled;
            set => Set(ref _gpsEnabled, value);
        }

        bool _cellEnabled;
        [JsonProperty("cellEnabled")]
        public bool CellEnabled
        {
            get => _cellEnabled;
            set => Set(ref _cellEnabled, value);
        }

        bool _wifiEnabled;
        [JsonProperty("wifiEnabled")]
        public bool WiFiEnabled
        {
            get => _wifiEnabled;
            set => Set(ref _wifiEnabled, value);
        }

        bool _verboseLogging;
        [JsonProperty("verboseLogging")]
        public bool VerboseLogging
        {
            get => _verboseLogging;
            set => Set(ref _verboseLogging, value);
        }

        int _pingRate;
        [JsonProperty("pingRate")]
        public int PingRate
        {
            get => _pingRate;
            set => Set(ref _pingRate, value);
        }


        int _sendUpdateRate;

        [JsonProperty("sendUpdateRate")]
        public int SendUpdateRate
        {
            get => _sendUpdateRate;
            set => Set(ref _sendUpdateRate, value);
        }

        int _gpsUpdateRate;
        [JsonProperty("gpsUpdateRate")]
        public int GPSUpdateRate
        {
            get => _gpsUpdateRate;
            set => Set(ref _gpsUpdateRate, value);
        }

        long _modemBaudRate;
        [JsonProperty("baud")]
        public long GPRSModemBaudRate
        {
            get => _modemBaudRate;
            set => Set(ref _modemBaudRate, value);
        }

    }
}
