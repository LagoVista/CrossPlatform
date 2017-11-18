using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Devices.SerialCommunication;
using LagoVista.Core.Models;

namespace LagoVista.Core.UWP.Services
{
    public class SerialPort : ISerialPort
    {
        SerialDevice _serialDevice;

        SerialPortInfo _portInfo;

        public SerialPort(SerialPortInfo info)
        {
            _portInfo = info;
        }

        public bool IsConnected
        {
            get { return _serialDevice != null; }
        }

        public Task CloseAsync()
        {
            Dispose();
            return Task.FromResult(default(object));
        }

        public void Dispose()
        {
            lock(this)
            {
                if(_serialDevice != null)
                {
                    _serialDevice.Dispose();
                    _serialDevice = null;
                }
            }
            throw new NotImplementedException();
        }

        public async Task OpenAsync()
        {
            _serialDevice = await SerialDevice.FromIdAsync(_portInfo.Id);
            _serialDevice.BaudRate = (uint)_portInfo.BaudRate;
        }

        public Stream InputStream
        {
            get
            {
                return _serialDevice.InputStream.AsStreamForRead();
            }
        }

        public Stream OutputStream { get { return _serialDevice.OutputStream.AsStreamForWrite();} }
    }
}
