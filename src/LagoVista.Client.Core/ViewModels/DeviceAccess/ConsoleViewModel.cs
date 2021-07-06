using LagoVista.Client.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class ConsoleViewModel : DeviceViewModelBase
    {

        public ConsoleViewModel()
        {

        }

        protected override void OnBLEDevice_Disconnected(BLEDevice device)
        {
            Lines.Insert(0, "Device Disconnected");
        }

        protected override void OnBTSerail_LineReceived(string line)
        {
            var cleanLine = line.Trim();
            if (!String.IsNullOrEmpty(cleanLine))
                Lines.Insert(0, cleanLine);
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            if (DeviceConnected)
            {
                Lines.Insert(0, "device connected.");
            }

        }

        public ObservableCollection<string> Lines { get; } = new ObservableCollection<string>();
    }
}
