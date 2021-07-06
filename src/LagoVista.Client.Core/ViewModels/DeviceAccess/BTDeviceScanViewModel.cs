﻿using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels.DeviceAccess.Settings;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class BTDeviceScanViewModel : DeviceViewModelBase
    {
        public BTDeviceScanViewModel()
        {
            StartScanCommand = new RelayCommand(StartScan, () => !IsScanning);
            StopScanCommand = new RelayCommand(StopScan, () => IsScanning);
        }

        public async override Task ReloadedAsync()
        {
            foreach(var connection in GattConnection.ConnectedDevices)
            {
                await GattConnection.DisconnectAsync(connection);
            }

            await base.ReloadedAsync();
        }

        public async void StartScan()
        {
            await GattConnection.StartScanAsync();
            IsScanning = true;
        }

        public async void StopScan()
        {
            await GattConnection.StopScanAsync();
            IsScanning = false;
        }

        bool _isScanning;
        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                Set(ref _isScanning, value);
                StartScanCommand.RaiseCanExecuteChanged();
                StopScanCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand StartScanCommand { get; }

        public RelayCommand StopScanCommand { get; }

        protected override void OnBLEDevice_Connected(BLEDevice device)
        {
            StopScan();
            GattConnection.RegisterKnownServices(NuvIoTGATTProfile.GetNuvIoTGATT().Services);
            this.NavigateAndViewAsync<DeviceSettingsViewModel>(device.DeviceAddress);
        }

        public async void ConnectAsync(BLEDevice device)
        {
            await GattConnection.ConnectAsync(device);
        }

        BLEDevice _selectedItem;
        public BLEDevice SelectedItem
        {
            get => _selectedItem;
            set
            {
                Set(ref _selectedItem, value);
                ConnectAsync(_selectedItem);
                _selectedItem = null;
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }
    }
}

