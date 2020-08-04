using LagoVista.Client.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.Interfaces
{
    public interface IBluetoothSerial
    {
        event EventHandler<string> ReceivedLine;

        event EventHandler<BTDevice> DeviceFound;

        event EventHandler<int> DFUProgress;

        event EventHandler DFUCompleted;
        event EventHandler<string> DFUFailed;

        event EventHandler<BTDevice> DeviceConnected;
        event EventHandler<BTDevice> DeviceConnecting;
        event EventHandler<BTDevice> DeviceDisconnected;

        Task<ObservableCollection<BTDevice>> SearchAsync();

        Task ConnectAsync(BTDevice device);

        Task SendDFUAsync(BTDevice device, byte[] firmware);

        Task SendLineAsync(String msg);
    }
}
