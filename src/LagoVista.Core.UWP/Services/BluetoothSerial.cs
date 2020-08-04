using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace LagoVista.Core.UWP.Services
{
    public class BluetoothSerial : IBluetoothSerial
    {
        private RfcommDeviceService _service;
        private StreamSocket _socket;
        private DataWriter dataWriterObject;
        private DataReader dataReaderObject;

        private CancellationTokenSource ReadCancellationTokenSource;

        public event EventHandler<string> ReceivedLine;
        public event EventHandler<BTDevice> DeviceFound;
        public event EventHandler<int> DFUProgress;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> DFUFailed;
        public event EventHandler<BTDevice> DeviceConnected;
        public event EventHandler<BTDevice> DeviceConnecting;
        public event EventHandler<BTDevice> DeviceDisconnected;

        public async Task ConnectAsync(BTDevice device)
        {
            _service = await RfcommDeviceService.FromIdAsync(device.DeviceId);

            _socket = new StreamSocket();
            await _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName);
            dataWriterObject = new DataWriter(_socket.OutputStream);
            Listen();
        }

        public async Task<ObservableCollection<BTDevice>> SearchAsync()
        {
            var deviceInfo = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

            var pairedDevices = new ObservableCollection<BTDevice>();
            foreach (var device in deviceInfo)
            {
                var service = await RfcommDeviceService.FromIdAsync(device.Id);


                pairedDevices.Add(new BTDevice()
                {
                    DeviceName = service.Device.Name,
                    DeviceId = device.Id
                });
            }


            return pairedDevices;
        }

        public Task SendDFUAsync(BTDevice device, byte[] firmware)
        {
            throw new NotImplementedException();
        }

        public async Task SendLineAsync(string msg)
        {
            Task<UInt32> storeAsyncTask;

            if (msg == "")
                msg = "none";// sendText.Text;
            if (msg.Length != 0)
            //if (msg.sendText.Text.Length != 0)
            {
                // Load the text from the sendText input text box to the dataWriter object
                dataWriterObject.WriteString(msg);

                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriterObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    string status_Text = msg + ", ";
                    status_Text += bytesWritten.ToString();
                    status_Text += " bytes written successfully!";
                    System.Diagnostics.Debug.WriteLine(status_Text);
                }
            }
            else
            {
                string status_Text2 = "Enter the text you want to write and then click on 'WRITE'";
                System.Diagnostics.Debug.WriteLine(status_Text2);
            }
        }


        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                ReadCancellationTokenSource = new CancellationTokenSource();
                if (_socket.InputStream != null)
                {
                    dataReaderObject = new DataReader(_socket.InputStream);
                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            cancellationToken.ThrowIfCancellationRequested();

            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                try
                {
                    string recvdtxt = dataReaderObject.ReadString(bytesRead);
                    Debug.WriteLine(recvdtxt);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("ReadAsync: " + ex.Message);
                }

            }
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }


    }
}
