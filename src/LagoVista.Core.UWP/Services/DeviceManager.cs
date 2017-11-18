using LagoVista.Core.PlatformSupport;
using System;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using System.Collections.ObjectModel;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;

namespace LagoVista.Core.UWP.Services
{
    public class DeviceManager : IDeviceManager
    {
        public ISerialPort CreateSerialPort(SerialPortInfo portInfo)
        {
            return new SerialPort(portInfo);
        }

        public async Task<ObservableCollection<SerialPortInfo>> GetSerialPortsAsync()
        {
            var ports = new ObservableCollection<SerialPortInfo>(); 
            var aqs = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(aqs);
            foreach(var device in devices)
            {
                ports.Add(new SerialPortInfo()
                {
                     Id = device.Id,
                     Name = device.Name,                      
                });
            }

            return ports;
        }
    }
}
