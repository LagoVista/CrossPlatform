using LagoVista.Client.Core.Models;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace SeaWolf
{
    public static class CommonExtensions
    {
        public static void AddValidSensors(this ObservableCollection<SensorSummary> sensors, IAppConfig appConfig, Device device)
        {
            sensors.Clear();
            /*var configs = device.Sensors.AdcConfigs;
            var values = device.Sensors.AdcValues;
            var validConfigs = configs.Where(adc => adc.Config > 0);

            
            foreach (var config in validConfigs)
            {
                sensors.Add(new SensorSummary()
                {
                    Config = config,
                    SensorType = appConfig.AppSpecificSensorTypes.FirstOrDefault(sns => sns.Key == config.Key),
                    Value = values[config.SensorIndex - 1].ToString(),
                });
            }

            configs = device.Sensors.IoConfigs;
            values = device.Sensors.IoValues;
            validConfigs = configs.Where(adc => adc.Config > 0);

            foreach (var config in validConfigs)
            {
                sensors.Add(new SensorSummary()
                {
                    Config = config,
                    SensorType = appConfig.AppSpecificSensorTypes.FirstOrDefault(sns => sns.Key == config.Key),
                    Value = values[config.SensorIndex - 1].ToString(),
                });
            }*/
        }
    }
}
