using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class DeviceViewModelBase : AppViewModelBase
    {
        private String _deviceRepoId;
        private String _deviceId;

        private readonly IGATTConnection _gattConnection;

        private BLEDevice _currentDevice;

        public const string DEVICE_ID = "DEVICE_ID";
        public const string DEVICE_REPO_ID = "DEVICE_REPO_ID";
        public const string BT_DEVICE_ADDRESS = "BT_ADDRESS";

        public DeviceViewModelBase()
        {
            if(SLWIOC.Contains(typeof(IGATTConnection)))
            {
                _gattConnection = SLWIOC.Get<IGATTConnection>();
            }

            
        }

        public async override Task InitAsync()
        {
            if (_gattConnection != null)
            {
                _gattConnection.DeviceConnected += BtSerial_DeviceConnected;
                _gattConnection.DeviceDisconnected += BtSerial_DeviceDisconnected;
                _gattConnection.ReceiveConsoleOut += BtSerial_ReceivedLine;
                _gattConnection.DFUProgress += BtSerial_DFUProgres;
                _gattConnection.DFUFailed += BtSerial_DFUFailed;
                _gattConnection.DFUCompleted += BtSerial_DFUCompleted;

                if (this.LaunchArgs.Parameters.ContainsKey(BT_DEVICE_ADDRESS))
                {
                    var btAddress = this.LaunchArgs.Parameters[BT_DEVICE_ADDRESS] as String;
                    CurrentDevice = _gattConnection.DiscoveredDevices.Where(dvc => dvc.DeviceAddress == btAddress).FirstOrDefault();
                }
            }

            await base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            if (_gattConnection != null)
            {
                _gattConnection.DeviceConnected += BtSerial_DeviceConnected;
                _gattConnection.DeviceDisconnected += BtSerial_DeviceDisconnected;
                _gattConnection.ReceiveConsoleOut += BtSerial_ReceivedLine;
                _gattConnection.DFUProgress += BtSerial_DFUProgres;
                _gattConnection.DFUFailed += BtSerial_DFUFailed;
                _gattConnection.DFUCompleted += BtSerial_DFUCompleted;
            }

            return base.ReloadedAsync();
        }


        protected virtual void OnDFUCompleted() { }
        protected virtual void OnDFUFailed(String err) { }
        protected virtual void OnDFUProgress(Models.DFUProgress e) { }


        private void BtSerial_DFUCompleted(object sender, EventArgs e)
        {
            OnDFUCompleted();
        }

        private void BtSerial_DFUFailed(object sender, string e)
        {
            OnDFUFailed(e);
        }

        private void BtSerial_DFUProgres(object sender, Models.DFUProgress e)
        {
            OnDFUProgress(e);    
        }

        private void BtSerial_ReceivedLine(object sender, string e)
        {
            OnBTSerail_MsgReceived(e);

            var lines = e.Split('\n');
            foreach (var line in lines)
            {
                OnBTSerail_LineReceived(line);
            }
        }

        private void BtSerial_DeviceDisconnected(object sender, Models.BLEDevice e)
        {
            OnBLEDevice_Disconnected(e);
            RaisePropertyChanged(nameof(DeviceConnected));
        }

        private void BtSerial_DeviceConnected(object sender, Models.BLEDevice e)
        {
            OnBLEDevice_Connected(e);
            RaisePropertyChanged(nameof(DeviceConnected));
        }

        public String DeviceId
        {
            get
            {
                if (String.IsNullOrEmpty(_deviceId) && LaunchArgs.Parameters.ContainsKey(DEVICE_ID))
                {

                    _deviceId = LaunchArgs.Parameters[DEVICE_ID].ToString() ?? throw new ArgumentNullException(nameof(DeviceViewModel.DeviceId));
                }

                return _deviceId;
            }
            set
            {
                _deviceId = value;
            }
        }

        public BLEDevice CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        protected Task SendAsync(String msg)
        {
            return Task.CompletedTask;   
        }

        protected Task DisconnectAsync()
        {
            return _gattConnection.DisconnectAsync(_currentDevice);
        }

        protected bool DeviceConnected
        {
            get { return _currentDevice.Connected;  }
        }

        protected virtual void OnBTSerail_MsgReceived(string msg) { }

        protected virtual void OnBTSerail_LineReceived(string line) { }
        protected virtual void OnBLEDevice_Connected(BLEDevice device){ }
        protected virtual void OnBLEDevice_Disconnected(BLEDevice device){ }

        protected virtual void SendDFU(byte[] buffer)
        {
            //_gattConnection.SendDFUAsync(buffer);
        }

        // Bit of a hack, if we are going from a device view to a child view
        // we don't want to disconnect so we keep the BT connection alive.
        // set a nasty flag to determine if this is the case.
        private bool _isShowingNewView = false;

        public async void DisconnectBTDevice()
        {
            if(!_isShowingNewView && _currentDevice.Connected)
            {
                await _gattConnection.DisconnectAsync(_currentDevice);
            }

            _isShowingNewView = false;
        }

        protected string DeviceRepoId
        {
            get
            {
                if (String.IsNullOrEmpty(_deviceRepoId))
                {
                    _deviceRepoId = LaunchArgs.Parameters[DEVICE_REPO_ID].ToString() ?? throw new ArgumentNullException(nameof(DeviceViewModel.DeviceRepoId));
                }

                return _deviceRepoId;
            }
        }

        protected void ShowView<TViewModel>()
        {
            _isShowingNewView = true;

            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(TViewModel),
                LaunchType = LaunchTypes.View
            };

            launchArgs.Parameters.Add(DEVICE_ID, DeviceId);
            launchArgs.Parameters.Add(DEVICE_REPO_ID, DeviceRepoId);

            ViewModelNavigation.NavigateAsync(launchArgs);
        }

        public override Task IsClosingAsync()
        {
            _gattConnection.DeviceConnected -= BtSerial_DeviceConnected;
            _gattConnection.DeviceDisconnected -= BtSerial_DeviceDisconnected;
            _gattConnection.ReceiveConsoleOut -= BtSerial_ReceivedLine;
            _gattConnection.DFUProgress -= BtSerial_DFUProgres;
            _gattConnection.DFUFailed -= BtSerial_DFUFailed;
            _gattConnection.DFUCompleted -= BtSerial_DFUCompleted;

            return base.IsClosingAsync();
        }

        public IGATTConnection GattConnection => _gattConnection;
    }
}
