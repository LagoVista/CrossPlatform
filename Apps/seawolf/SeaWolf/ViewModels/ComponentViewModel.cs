using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using Newtonsoft.Json;
using SeaWolf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class ComponentViewModel : MonitoringViewModelBase
    {
        public const string DeviceRepoId = "DEVICEREPOID";
        public const string DeviceId = "DEVICEID";

        string _deviceRepoId;
        string _deviceId;

        Device _device;

        List<InputCommandEndPoint> _inputCommandEndPoints;

        public ComponentViewModel()
        {
            DeviceMessages = new ObservableCollection<DeviceArchive>();
            SendCommand = new RelayCommand(Send);
            CancelSendCommand = new RelayCommand(CancelSend);
            ShowSettingsCommand = new RelayCommand(ShowConfigurationView);
            Components = new ObservableCollection<ComponentBase>();
            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    Command = new RelayCommand(() => ShowLiveDataView()),
                    Name = "Live Data",
                    FontIconKey = "fa-info"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => CloseScreen()),
                    Name = SeawolfResources.Common_Exit,
                    FontIconKey = "fa-sign-out"
                }
            };
        }

        private void ShowConfigurationView()
        {
            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(ConfigurationViewModel),
                LaunchType = LaunchTypes.Edit
            };

            launchArgs.Parameters.Add(ComponentViewModel.DeviceRepoId, _deviceRepoId);
            launchArgs.Parameters.Add(ComponentViewModel.DeviceId, _deviceId);

            ViewModelNavigation.NavigateAsync(launchArgs);
        }

        private void ShowLiveDataView()
        {
            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(LiveDataViewModel),
                LaunchType = LaunchTypes.Edit
            };

            launchArgs.Parameters.Add(ComponentViewModel.DeviceRepoId, _deviceRepoId);
            launchArgs.Parameters.Add(ComponentViewModel.DeviceId, _deviceId);

            ViewModelNavigation.NavigateAsync(launchArgs);

        }


        public ObservableCollection<Models.ComponentBase> Components { get; private set; }

        public async override Task InitAsync()
        {
            _deviceRepoId = LaunchArgs.Parameters[DeviceRepoId].ToString();
            _deviceId = LaunchArgs.Parameters[DeviceId].ToString();

            await base.InitAsync();

            await PerformNetworkOperation(async () =>
            {
                var path = $"/api/device/{_deviceRepoId}/{_deviceId}/metadata";

                var response1 = await RestClient.GetAsync(path);
                Debug.WriteLine(response1.Content);

                var response = await RestClient.GetAsync<DetailResponse<Device>>(path);
                if (response.Successful)
                {
                    Device = response.Result.Model;
                    foreach (var endpoint in Device.InputCommandEndPoints)
                    {
                        Debug.WriteLine(endpoint.InputCommand.Name);
                        Debug.WriteLine(endpoint.EndPoint);
                    }
                    ViewReady = true;
                }
            });
        }

        public async void Send()
        {
            await PerformNetworkOperation(async () =>
            {
                var queryString = String.Empty;
                foreach (var param in InputCommandParameters)
                {
                    if (!String.IsNullOrEmpty(queryString))
                    {
                        queryString += "&";
                    }

                    queryString += param.ToQueryStringPair();
                }

                var client = new HttpClient();
                var endPoint = _inputCommandEndPoints.Where(inp => inp.InputCommand.Key == SelectedInputCommand.Key).FirstOrDefault();
                if (endPoint != null)
                {
                    var uri = String.IsNullOrEmpty(queryString) ? endPoint.EndPoint : $"{endPoint.EndPoint}?{queryString}";
                    var result = await client.GetAsync(uri);
                    if (!result.IsSuccessStatusCode)
                    {
                        await Popups.ShowAsync(result.ReasonPhrase + $" ({result.StatusCode})");
                    }
                }
            });

            SelectedInputCommand = InputCommands.FirstOrDefault();
            DeviceStatusVisible = true;
            InputCommandVisible = false;
        }

        public void CancelSend()
        {
            SelectedInputCommand = InputCommands.FirstOrDefault();
            DeviceStatusVisible = true;
            InputCommandVisible = false;
        }


        public override void HandleMessage(Notification notification)
        {
            if (!String.IsNullOrEmpty(notification.PayloadType))
            {
                switch (notification.PayloadType)
                {
                    case "DeviceArchive":
                        var archive = JsonConvert.DeserializeObject<DeviceArchive>(notification.Payload);
                        DispatcherServices.Invoke(() =>
                        {
                            DeviceMessages.Insert(0, archive);
                            if (DeviceMessages.Count == 50)
                            {
                                DeviceMessages.RemoveAt(2);
                            }
                        });
                        break;
                    case "Device":
                        DispatcherServices.Invoke(() =>
                        {
                            Device = JsonConvert.DeserializeObject<Device>(notification.Payload);
                        });

                        break;
                }
                Debug.WriteLine("----");
                Debug.WriteLine(notification.PayloadType);
                Debug.WriteLine(notification.Payload);
                Debug.WriteLine("BYTES: " + notification.Payload.Length);
                Debug.WriteLine("----");
            }
            else
            {
                Debug.WriteLine(notification.Text);
            }
        }

        public override string GetChannelURI()
        {
            return $"/api/wsuri/device/{_deviceId}/normal";
            //            return "/api/wsuri/instance/5E78188E767349D681898F0AD8CD1FFC/normal";
        }

        private void PopulateComponents(Device device)
        {
            var properties = device.Properties;
            foreach (var prop in properties)
            {
                Debug.WriteLine($"{prop.Key} - {prop.Value}");
            }

            for (var idx = 1; idx < 7; ++idx)
            {
                var hasVoltage = device.Properties.Where(prop => prop.Key == $"hasv{idx}").FirstOrDefault();
                if (hasVoltage != null && hasVoltage.Value == "True")
                {
                    var voltageValue = device.Attributes.Where(attr => attr.Key == $"voltage{idx}").FirstOrDefault();
                    if (voltageValue != null)
                    {
                        var rawVoltage = Convert.ToDouble(voltageValue.Value);
                        var existingComponent = Components.FirstOrDefault(cmp => cmp.Key == voltageValue.Key);
                        if (existingComponent != null)
                        {
                            (existingComponent as VoltageComponent).Value = $"{rawVoltage:N1}V";
                        }
                        else
                        {
                            var voltageComponent = new VoltageComponent(voltageValue.Key)
                            {
                                Label = device.Properties.Where(prop => prop.Key == $"v{idx}label").First().Value,
                                Value = $"{rawVoltage:N2}V"
                            };

                            Components.Add(voltageComponent);
                        }
                    }
                }
            }

            for (var idx = 1; idx < 6; ++idx)
            {
                var hasTemperature = device.Properties.Where(prop => prop.Key == $"hastemperature{idx}").FirstOrDefault();
                if (hasTemperature != null && hasTemperature.Value == "True")
                {
                    var temperatureValue = device.Attributes.Where(attr => attr.Key == $"temperature{idx}").FirstOrDefault();
                    if (temperatureValue != null)
                    {
                        var existingComponent = Components.FirstOrDefault(cmp => cmp.Key == temperatureValue.Key);
                        if (existingComponent != null)
                        {
                            (existingComponent as VoltageComponent).Value = $"{temperatureValue.Value}F";
                        }
                        else {
                            var temperatureComponent = new TemperatureComponent(temperatureValue.Key)
                            {
                                Label = device.Properties.Where(prop => prop.Key == $"temperature{idx}label").First().Value,
                                Value = $"{temperatureValue.Value}F"
                            };

                            Components.Add(temperatureComponent);
                        }
                    }
                }
            }

        }

        private string _lastUpdated;
        public string LastUpdated
        {
            get { return _lastUpdated; }
            set { Set(ref _lastUpdated, value); }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { Set(ref _deviceName, value); }
        }

        public Device Device
        {
            get { return _device; }
            set
            {
                LastUpdated = DateTime.Now.ToString();
                DeviceName = value.Name;

                var attrs = new ObservableCollection<KeyValuePair<string, object>>();
                //var properties = new ObservableCollection<KeyValuePair<string, CustomField>>();
                var stateMachines = new ObservableCollection<KeyValuePair<string, object>>();
                var inputCommands = new ObservableCollection<InputCommand>();

                if (value != null)
                {
                    var properties = value.Properties;
                    foreach (var prop in properties)
                    {
                        Debug.WriteLine($"{prop.Key} - {prop.Value}");
                    }

                    foreach (var attr in value.Attributes)
                    {
                        attrs.Add(new KeyValuePair<string, object>(attr.Name, attr.Value));
                    }

                    foreach (var stm in value.States)
                    {
                        var state = stm.StateSet.Value.States.Where(stat => stat.Key == stm.Value).FirstOrDefault();
                        if (state != null)
                        {
                            stateMachines.Add(new KeyValuePair<string, object>(stm.Name, state.Name));
                        }
                        else
                        {
                            stateMachines.Add(new KeyValuePair<string, object>(stm.Name, stm.Value));
                        }
                    }



                    /* We only get these when the device is first loaded, not when the device
                     * is updated via web socket */
                    if (value.InputCommandEndPoints != null)
                    {
                        _inputCommandEndPoints = value.InputCommandEndPoints;
                        foreach (var cmd in _inputCommandEndPoints)
                        {
                            inputCommands.Add(cmd.InputCommand);
                        }

                        HasInputCommands = inputCommands.Count > 0;
                        if (HasInputCommands)
                        {
                            inputCommands.Insert(0, new InputCommand()
                            {
                                Key = "N/A",
                                Name = SeawolfResources.Common_SelectCommand
                            });
                            InputCommands = inputCommands;
                            SelectedInputCommand = inputCommands[0];
                        }
                        else
                        {
                            SelectedInputCommand = null;
                        }
                    }

                    PopulateComponents(value);
                }

                foreach (var attr in attrs)
                {

                }

                foreach (var attr in attrs)
                {

                }

                StateMachines = stateMachines;
                HasStateMachines = StateMachines.Any();
                HasAttributes = attrs.Any();
                DeviceAttributes = attrs;
                Set(ref _device, value);
            }
        }

        private void ShowInputCommand(InputCommand inputCommand)
        {
            InputCommandVisible = true;
            DeviceStatusVisible = false;

            InputCommandParameters = new ObservableCollection<InputCommandParameter>();
            foreach (var param in inputCommand.Parameters)
            {
                InputCommandParameters.Add(new InputCommandParameter(param));
            }
        }

        public bool _viewReady = false;
        public bool ViewReady
        {
            get { return _viewReady; }
            set { Set(ref _viewReady, value); }
        }

        public RelayCommand EditDeviceCommand { get; private set; }

        private ObservableCollection<DeviceArchive> _deviceMessasges;
        public ObservableCollection<DeviceArchive> DeviceMessages
        {
            get { return _deviceMessasges; }
            set { Set(ref _deviceMessasges, value); }
        }

        ObservableCollection<KeyValuePair<string, object>> _stateMachines;
        public ObservableCollection<KeyValuePair<string, object>> StateMachines
        {
            get { return _stateMachines; }
            set { Set(ref _stateMachines, value); }
        }

        ObservableCollection<KeyValuePair<string, object>> _deviceAttributes;
        public ObservableCollection<KeyValuePair<string, object>> DeviceAttributes
        {
            get { return _deviceAttributes; }
            set { Set(ref _deviceAttributes, value); }
        }

        public ObservableCollection<InputCommand> _inputCommands;
        public ObservableCollection<InputCommand> InputCommands
        {
            get { return _inputCommands; }
            set { Set(ref _inputCommands, value); }
        }

        InputCommand _selectedInputCommand;
        public InputCommand SelectedInputCommand
        {
            get { return _selectedInputCommand; }
            set
            {
                if (value != null)
                {
                    if (value != InputCommands.FirstOrDefault())
                    {
                        ShowInputCommand(value);
                    }
                }

                Set(ref _selectedInputCommand, value);
            }
        }

        RelayCommand _sendCommand;
        public RelayCommand SendCommand
        {
            get { return _sendCommand; }
            set { Set(ref _sendCommand, value); }
        }


        RelayCommand _cacnelSendCommand;
        public RelayCommand CancelSendCommand
        {
            get { return _cacnelSendCommand; }
            set { Set(ref _cacnelSendCommand, value); }
        }

        private bool _hasInputCommands = false;
        public bool HasInputCommands
        {
            get { return _hasInputCommands; }
            set { Set(ref _hasInputCommands, value); }
        }

        private bool _hasStateMachines = false;
        public bool HasStateMachines
        {
            get { return _hasStateMachines; }
            set { Set(ref _hasStateMachines, value); }
        }

        private bool _hasAttributes = false;
        public bool HasAttributes
        {
            get { return _hasAttributes; }
            set { Set(ref _hasAttributes, value); }
        }

        private bool _inputCommandVisible = false;
        public bool InputCommandVisible
        {
            get { return _inputCommandVisible; }
            set { Set(ref _inputCommandVisible, value); }
        }

        private bool _deviceStatusVisible = true;
        public bool DeviceStatusVisible
        {
            get { return _deviceStatusVisible; }
            set { Set(ref _deviceStatusVisible, value); }
        }

        ObservableCollection<InputCommandParameter> _inputCommandParameters;
        public ObservableCollection<InputCommandParameter> InputCommandParameters
        {
            get { return _inputCommandParameters; }
            set { Set(ref _inputCommandParameters, value); }
        }

        public RelayCommand ShowSettingsCommand { get; private set; }
    }
}
