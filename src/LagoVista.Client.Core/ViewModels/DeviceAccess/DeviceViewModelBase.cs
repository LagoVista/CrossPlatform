using LagoVista.Client.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using System;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class DeviceViewModelBase : AppViewModelBase
    {
        private String _deviceRepoId;
        private String _deviceId;

        private IBluetoothSerial _btSerial;

        public const string DEVICE_ID = "DEVICE_ID";
        public const string DEVICE_REPO_ID = "DEVICE_REPO_ID";

        public DeviceViewModelBase()
        {
            _btSerial = SLWIOC.Get<IBluetoothSerial>();
        }

        public async override Task InitAsync()
        {
            _btSerial.DeviceConnected += _btSerial_DeviceConnected;
            _btSerial.DeviceDisconnected += _btSerial_DeviceDisconnected;
            _btSerial.ReceivedLine += _btSerial_ReceivedLine;
            _btSerial.DFUProgress += _btSerial_DFUProgress;
            _btSerial.DFUFailed += _btSerial_DFUFailed;
            _btSerial.DFUCompleted += _btSerial_DFUCompleted;

            await base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            _btSerial.DeviceConnected += _btSerial_DeviceConnected;
            _btSerial.DeviceDisconnected += _btSerial_DeviceDisconnected;
            _btSerial.ReceivedLine += _btSerial_ReceivedLine;
            _btSerial.DFUProgress += _btSerial_DFUProgress;
            _btSerial.DFUFailed += _btSerial_DFUFailed;
            _btSerial.DFUCompleted += _btSerial_DFUCompleted;

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

        private void _btSerial_DeviceDisconnected(object sender, Models.BTDevice e)
        {
            OnBTSerial_DeviceDisconnected();
            RaisePropertyChanged(nameof(DeviceConnected));
        }

        private void _btSerial_DeviceConnected(object sender, Models.BTDevice e)
        {
            OnBTSerial_DeviceConnected();
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

        protected Task SendAsync(String msg)
        {
            return _btSerial.SendAsync(msg);
        }

        protected Task DisconnectAsync()
        {
            return _btSerial.DisconnectAsync();
        }

        protected bool DeviceConnected
        {
            get { return _btSerial.IsConnected;  }
        }

        protected virtual void OnBTSerail_MsgReceived(string msg) { }

        protected virtual void OnBTSerail_LineReceived(string line) { }
        protected virtual void OnBTSerial_DeviceConnected(){ }
        protected virtual void OnBTSerial_DeviceDisconnected(){ }

        protected virtual void SendDFU(byte[] buffer)
        {
            _btSerial.SendDFUAsync(buffer);
        }

        // Bit of a hack, if we are going from a device view to a child view
        // we don't want to disconnect so we keep the BT connection alive.
        // set a nasty flag to determine if this is the case.
        private bool _isShowingNewView = false;

        public async void DisconnectBTDevice()
        {
            if(!_isShowingNewView && _btSerial.IsConnected)
            {
                await _btSerial.DisconnectAsync();
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
            _btSerial.DeviceConnected -= _btSerial_DeviceConnected;
            _btSerial.DeviceDisconnected -= _btSerial_DeviceDisconnected;
            _btSerial.ReceivedLine -= _btSerial_ReceivedLine;
            _btSerial.DFUProgress -= _btSerial_DFUProgress;
            _btSerial.DFUFailed -= _btSerial_DFUFailed;
            _btSerial.DFUCompleted -= _btSerial_DFUCompleted;

            return base.IsClosingAsync();
        }

    }
}
