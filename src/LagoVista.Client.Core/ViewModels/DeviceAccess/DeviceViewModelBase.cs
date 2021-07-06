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

        private IGATTConnection _gattConnection;

        private BLEDevice _currentDevice;

        public const string DEVICE_ID = "DEVICE_ID";
        public const string DEVICE_REPO_ID = "DEVICE_REPO_ID";
        public const string BT_DEVICE_ADDRESS = "BT_ADDRESS";

        public DeviceViewModelBase()
        {
            _gattConnection = SLWIOC.Get<IGATTConnection>();
        }

        public async override Task InitAsync()
        {
            _gattConnection.DeviceConnected += _btSerial_DeviceConnected;
            _gattConnection.DeviceDisconnected += _btSerial_DeviceDisconnected;
            _gattConnection.ReceiveConsoleOut += _btSerial_ReceivedLine;
            _gattConnection.DFUProgress += _btSerial_DFUProgress;
            _gattConnection.DFUFailed += _btSerial_DFUFailed;
            _gattConnection.DFUCompleted += _btSerial_DFUCompleted;

            if(this.LaunchArgs.Parameters.ContainsKey(BT_DEVICE_ADDRESS))
            {
                var btAddress = this.LaunchArgs.Parameters[BT_DEVICE_ADDRESS] as String;
                CurrentDevice = _gattConnection.DiscoveredDevices.Where(dvc => dvc.DeviceAddress == btAddress).FirstOrDefault();
            }            

            await base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            _gattConnection.DeviceConnected += _btSerial_DeviceConnected;
            _gattConnection.DeviceDisconnected += _btSerial_DeviceDisconnected;
            _gattConnection.ReceiveConsoleOut += _btSerial_ReceivedLine;
            _gattConnection.DFUProgress += _btSerial_DFUProgress;
            _gattConnection.DFUFailed += _btSerial_DFUFailed;
            _gattConnection.DFUCompleted += _btSerial_DFUCompleted;

            return base.ReloadedAsync();
        }


        protected virtual void OnDFUCompleted() { }
        protected virtual void OnDFUFailed(String err) { }
        protected virtual void OnDFUProgress(Models.DFUProgress e) { }


        private void _btSerial_DFUCompleted(object sender, EventArgs e)
        {
            OnDFUCompleted();
        }

        private void _btSerial_DFUFailed(object sender, string e)
        {
            OnDFUFailed(e);
        }

        private void _btSerial_DFUProgress(object sender, Models.DFUProgress e)
        {
            OnDFUProgress(e);    
        }

        private void _btSerial_ReceivedLine(object sender, string e)
        {
            OnBTSerail_MsgReceived(e);

            var lines = e.Split('\n');
            foreach (var line in lines)
            {
                OnBTSerail_LineReceived(line);
            }
        }

        private void _btSerial_DeviceDisconnected(object sender, Models.BLEDevice e)
        {
            OnBLEDevice_Disconnected(e);
            RaisePropertyChanged(nameof(DeviceConnected));
        }

        private void _btSerial_DeviceConnected(object sender, Models.BLEDevice e)
        {
            OnBLEDevice_Connected(e);
            RaisePropertyChanged(nameof(DeviceConnected));
        }

        public String DeviceId
        {
            get
            {
                if (String.IsNullOrEmpty(_deviceId))
                {
                    _deviceId = LaunchArgs.Parameters[DEVICE_ID].ToString() ?? throw new ArgumentNullException(nameof(DeviceViewModel.DeviceId));
                }

                return _deviceId;
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
            _gattConnection.DeviceConnected -= _btSerial_DeviceConnected;
            _gattConnection.DeviceDisconnected -= _btSerial_DeviceDisconnected;
            _gattConnection.ReceiveConsoleOut -= _btSerial_ReceivedLine;
            _gattConnection.DFUProgress -= _btSerial_DFUProgress;
            _gattConnection.DFUFailed -= _btSerial_DFUFailed;
            _gattConnection.DFUCompleted -= _btSerial_DFUCompleted;

            return base.IsClosingAsync();
        }

        public IGATTConnection GattConnection => _gattConnection;
    }
}
