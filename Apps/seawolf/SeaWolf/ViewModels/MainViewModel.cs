using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.DeviceManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class MainViewModel : ListViewModelBase<DeviceSummary>
    {
        public const string REPO_ID = "7D9871D47B7F4BDCB338FAE4C1CBF947";
        private double _lowTemperatureThreshold = 80;
        private double _highTemperatureThreshold = 120;

        private double _lowBatteryThreshold = 12.5;
        private double _highBatteryThreshold = 14.5;

        EntityHeader _temperatureSensor;
        EntityHeader _batterySensor;


        public MainViewModel()
        {
            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<UserOrgsViewModel>(this)),
                    Name = ClientResources.MainMenu_SwitchOrgs,
                    FontIconKey = "fa-users"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<AboutViewModel>(this)),
                    Name = "About",
                    FontIconKey = "fa-info"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => Logout()),
                    Name = ClientResources.Common_Logout,
                    FontIconKey = "fa-sign-out"
                }
            };

            IncrementHighBatteryThresholdCommand = new RelayCommand(IncrementHighBatteryThreshold, CanIncrementHighBatteryThreshold);
            IncrementLowBatteryThresholdCommand = new RelayCommand(IncrementLowBatteryThreshold, CanIncrementLowBatteryThreshold);
            DecrementHighBatteryThresholdCommand = new RelayCommand(DecrementHighBatteryThreshold, CanDecrementHighBatteryThreshold);
            DecrementLowBatteryThresholdCommand = new RelayCommand(DecrementLowBatteryThreshold, CanDecrementLowBatteryThreshold);
      
            IncrementHighTemperatureThresholdCommand = new RelayCommand(IncrementHighTemperatureThreshold, CanIncrementHighTemperatureThreshold);
            IncrementLowTemperatureThresholdCommand = new RelayCommand(IncrementLowTemperatureThreshold, CanIncrementLowTemperatureThreshold);
            DecrementHighTemperatureThresholdCommand = new RelayCommand(DecrementHighTemperatureThreshold, CanDecrementHighTemperatureThreshold);
            DecrementLowTemperatureThresholdCommand = new RelayCommand(DecrementLowTemperatureThreshold, CanDecrementLowTemperatureThreshold);

            TemperatureSensors.Add(new EntityHeader() { Id = "1", Text = "Live Well 1" });
            TemperatureSensors.Add(new EntityHeader() { Id = "2", Text = "Live Well 2" });
            TemperatureSensors.Add(new EntityHeader() { Id = "3", Text = "Trailer Hub - Driver" });
            TemperatureSensors.Add(new EntityHeader() { Id = "4", Text = "Trailer Hub - Passenger" });

            BatterySensors.Add(new EntityHeader() { Id = "1", Text = "Trolling Motor 1" });
            BatterySensors.Add(new EntityHeader() { Id = "2", Text = "Cranking" });
            BatterySensors.Add(new EntityHeader() { Id = "3", Text = "Aux 1" });
        }

        public override Task InitAsync()
        {
            return base.InitAsync();
        }

        protected override void ItemSelected(DeviceSummary model)
        {
            base.ItemSelected(model);
            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(ComponentViewModel),
                LaunchType = LaunchTypes.View
            };

            launchArgs.Parameters.Add(ComponentViewModel.DeviceRepoId, REPO_ID);
            launchArgs.Parameters.Add(ComponentViewModel.DeviceId, model.Id);

            SelectedItem = null;

            ViewModelNavigation.NavigateAsync(launchArgs);
        }

        private bool _hasDevices;
        public bool HasDevices
        {
            get { return _hasDevices; }
            set { Set(ref _hasDevices, value); }
        }

        private bool _noDevices;
        public bool NoDevices
        {
            get { return _noDevices; }
            set { Set(ref _noDevices, value); }
        }

        public ObservableCollection<EntityHeader> TemperatureSensors { get; } = new ObservableCollection<EntityHeader>();
        public ObservableCollection<EntityHeader> BatterySensors { get; } = new ObservableCollection<EntityHeader>();

        public EntityHeader TemperatureSensor
        {
            get => _temperatureSensor;
            set
            {
                Set(ref _temperatureSensor, value);
                IncrementHighTemperatureThresholdCommand.RaiseCanExecuteChanged();
                IncrementLowTemperatureThresholdCommand.RaiseCanExecuteChanged();
                DecrementHighTemperatureThresholdCommand.RaiseCanExecuteChanged();
                DecrementLowTemperatureThresholdCommand.RaiseCanExecuteChanged();
            }
        }

        public EntityHeader BatterySensor
        {
            get => _batterySensor;
            set
            {
                Set(ref _batterySensor, value);
                IncrementHighBatteryThresholdCommand.RaiseCanExecuteChanged();
                IncrementLowBatteryThresholdCommand.RaiseCanExecuteChanged();
                DecrementHighBatteryThresholdCommand.RaiseCanExecuteChanged();
                DecrementLowBatteryThresholdCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanIncrementHighBatteryThreshold(Object obj)
        {
            return BatterySensor != null;
        }

        public bool CanIncrementLowBatteryThreshold(Object obj)
        {
            return BatterySensor != null && HighBatteryTolerance > LowBatteryTolerance;
        }

        public bool CanDecrementHighBatteryThreshold(Object obj)
        {
            return BatterySensor != null && HighBatteryTolerance > LowBatteryTolerance;
        }

        public bool CanDecrementLowBatteryThreshold(Object obj)
        {
            return BatterySensor != null;
        }

        public bool CanIncrementHighTemperatureThreshold(Object obj)
        {
            return TemperatureSensor != null;
        }

        public bool CanIncrementLowTemperatureThreshold(Object obj)
        {
            return TemperatureSensor != null && HighTemperatureTolerance > LowTemperatureTolerance;
        }

        public bool CanDecrementHighTemperatureThreshold(Object obj)
        {
            return TemperatureSensor != null && HighTemperatureTolerance > LowTemperatureTolerance;
        }

        public bool CanDecrementLowTemperatureThreshold(Object obj)
        {
            return TemperatureSensor != null;
        }

        public void IncrementHighBatteryThreshold(object obj)
        {
            HighBatteryTolerance += .1;
        }

        public void IncrementLowBatteryThreshold(object obj)
        {
            LowBatteryTolerance -= .1;
        }

        public void DecrementHighBatteryThreshold(object obj)
        {
            HighBatteryTolerance -= .1;
        }

        public void DecrementLowBatteryThreshold(object obj)
        {
            LowBatteryTolerance -= .1;
        }


        public void IncrementHighTemperatureThreshold(object obj)
        {
            HighTemperatureTolerance += 0.5;
        }

        public void IncrementLowTemperatureThreshold(object obj)
        {
            LowTemperatureTolerance += 0.5;
        }

        public void DecrementHighTemperatureThreshold(object obj)
        {
            HighTemperatureTolerance -= 0.5;
        }

        public void DecrementLowTemperatureThreshold(object obj)
        {
            LowTemperatureTolerance -= 0.5;
        }

        public double HighBatteryTolerance
        {
            get => _highBatteryThreshold;
            set => Set(ref _highBatteryThreshold, value);
        }
        public double LowBatteryTolerance
        {
            get => _lowBatteryThreshold;
            set => Set(ref _lowBatteryThreshold, value);
        }

        public double HighTemperatureTolerance
        {
            get => _highTemperatureThreshold;
            set=> Set(ref _highTemperatureThreshold, value);
        }
        public double LowTemperatureTolerance
        {
            get => _lowTemperatureThreshold;
            set => Set(ref _lowTemperatureThreshold, value);
        }

        public RelayCommand IncrementHighTemperatureThresholdCommand { get; }
        public RelayCommand IncrementLowTemperatureThresholdCommand { get; }

        public RelayCommand DecrementHighTemperatureThresholdCommand { get; }
        public RelayCommand DecrementLowTemperatureThresholdCommand { get; }

        public RelayCommand IncrementHighBatteryThresholdCommand { get; }
        public RelayCommand IncrementLowBatteryThresholdCommand { get; }

        public RelayCommand DecrementHighBatteryThresholdCommand { get; }
        public RelayCommand DecrementLowBatteryThresholdCommand { get; }


        protected override string GetListURI()
        {
            return $"/api/devices/{REPO_ID}";
        }
    }
}
