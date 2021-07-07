using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Deployment.Admin.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using LagoVista.Core;
using Newtonsoft.Json;
using LagoVista.IoT.Runtime.Core.Models.Messaging;
using LagoVista.Client.Core.Net;
using LagoVista.Core.Validation;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;

namespace LagoVista.PlatformManager.Core.ViewModels
{
    public class MonitorInstanceViewModel : MonitoringViewModelBase
    {
        IWebSocket _hostWebSocket;

        enum ResourceType
        {
            Host,
            Instance,
        }

        public override Task InitAsync()
        {
            PerformNetworkOperation(async () =>
            {
                var uri = $"/api/deployment/instance/{LaunchArgs.ChildId}";
                var response = await RestClient.GetAsync<DetailResponse<DeploymentInstance>>(uri);
                if (response.Successful)
                {
                    Instance = response.Result.Model;

                    var hostUri = $"/api/deployment/host/{Instance.PrimaryHost.Id}";
                    var hostResponse = await RestClient.GetAsync<DetailResponse<DeploymentHost>>(hostUri);
                    if (hostResponse.Successful)
                    {
                        Host = hostResponse.Result.Model;
                        var hostChannelUri = $"/api/wsuri/host/{Instance.PrimaryHost.Id}/normal";

                        Debug.WriteLine("Asing for end URI: " + hostChannelUri);
                        var wsResult = await RestClient.GetAsync<InvokeResult<string>>(hostChannelUri);
                        if (wsResult.Successful)
                        {
                            var url = wsResult.Result.Result;
                            Debug.WriteLine(url);
                            var wsUri = new Uri(url);
                            _hostWebSocket = SLWIOC.Create<IWebSocket>();
                            _hostWebSocket.MessageReceived += _hostWebSocket_MessageReceived; ;
                            var wsOpenResult = await _hostWebSocket.OpenAsync(wsUri);
                            if (wsOpenResult.Successful)
                            {
                                Debug.WriteLine("OPENED CHANNEL");
                            }
                        }

                        EnableActions();
                    }
                    else
                    {
                        await ShowServerErrorMessageAsync(hostResponse.ToInvokeResult());
                        CloseScreen();
                    }
                }
                else
                {
                    await ShowServerErrorMessageAsync(response.ToInvokeResult());
                    CloseScreen();
                }

            });

            return base.InitAsync();
        }


        DeploymentHost _host;
        public DeploymentHost Host
        {
            get { return _host; }
            set { Set(ref _host, value); }
        }

        DeploymentInstance _instance;
        public DeploymentInstance Instance
        {
            get { return _instance; }
            set { Set(ref _instance, value); }
        }

        public override string GetChannelURI()
        {
            return $"/api/wsuri/instance/{LaunchArgs.ChildId}/normal";
        }

        private void SendAction(ResourceType resourceType, string action, string message)
        {
            PerformNetworkOperation(async () =>
            {
                var resourceId = resourceType == ResourceType.Host ? Instance.Host.Id : Instance.Id;
                var uri = $"/api/deployment/{resourceType.ToString().ToLower()}/{resourceId}/{action}";
                Debug.WriteLine("Request Id: " + uri);

                var response = await RestClient.GetAsync(uri);
                if (response.Success)
                {
                    AddMessage(DateTime.UtcNow, message);
                }
                else
                {
                    await ShowServerErrorMessageAsync(response.ToInvokeResult());
                    AddMessage(DateTime.UtcNow, response.ErrorMessage);
                }
            });
        }

        private void AddMessage(Client.Core.Models.Notification notification)
        {
            if (!String.IsNullOrEmpty(notification.Text))
            {
                DispatcherServices.Invoke(() =>
                {
                    MessagesFromServer.Insert(0, notification);
                    if (MessagesFromServer.Count > 100) MessagesFromServer.RemoveAt(100);
                });
            }
        }

        private void AddMessage(DateTime dateStamp, string message)
        {
            var notification = new Client.Core.Models.Notification()
            {
                Text = message,
                DateStamp = dateStamp.ToJSONString()
            };

            AddMessage(notification);
        }

        private void EnableActions()
        {
            DispatcherServices.Invoke(() =>
            {
                CanDeployHost = false;
                CanPauseApplication = false;
                CanReloadSolution = false;
                CanRemoveServer = false;
                CanRestartServer = false;
                CanRestartContainer = false;
                CanStartApplication = false;
                CanStopApplication = false;
                CanUpdateRuntime = false;


                ShowIsBusy = false;

                switch (Host.Status.Value)
                {
                    case HostStatus.Offline:
                        CanDeployHost = true;
                        break;
                    case HostStatus.FailedDeployment:
                    case HostStatus.HostHealthCheckFailed:
                        CanRestartServer = true;
                        CanRemoveServer = true;
                        CanUpdateRuntime = true;
                        break;
                    case HostStatus.Running:
                        CanRestartServer = true;
                        CanRemoveServer = true;
                        CanUpdateRuntime = true;
                        switch (Instance.Status.Value)
                        {
                            case DeploymentInstanceStates.FatalError:
                            case DeploymentInstanceStates.FailedToInitialize:
                                CanRestartContainer = true;
                                CanReloadSolution = true;
                                break;
                            case DeploymentInstanceStates.Offline:
                                CanRestartContainer = true;
                                CanReloadSolution = true;
                                CanStartApplication = true;
                                CanReloadSolution = true;
                                break;
                            case DeploymentInstanceStates.Paused:
                                CanRestartContainer = true;
                                CanReloadSolution = true;
                                CanStopApplication = true;
                                CanStartApplication = true;
                                break;
                            case DeploymentInstanceStates.Stopped:
                                CanRestartContainer = true;
                                CanReloadSolution = true;
                                CanStartApplication = true;
                                break;
                            case DeploymentInstanceStates.Running:
                                CanRestartContainer = true;
                                CanReloadSolution = true;
                                CanPauseApplication = true;
                                CanStopApplication = true;
                                break;
                            default:
                                ShowIsBusy = true;
                                break;
                        }
                        break;
                    default:
                        ShowIsBusy = true;
                        break;

                }
            });
        }

        public override void HandleMessage(Client.Core.Models.Notification notification)
        {
            if (!String.IsNullOrEmpty(notification.PayloadType))
            {
                Debug.WriteLine("----");
                Debug.WriteLine(notification.PayloadType);
                Debug.WriteLine(notification.Payload);
                Debug.WriteLine("----");

                switch (notification.PayloadType)
                {
                    case "StateChangeNotification":
                        var stateChange = JsonConvert.DeserializeObject<StateChangeNotification>(notification.Payload);
                        var titleCasedId = $"{stateChange.NewState.Id.Substring(0, 1).ToUpper()}{stateChange.NewState.Id.Substring(1)}";

                        Instance.Status = new EntityHeader<DeploymentInstanceStates>()
                        {
                            Id = stateChange.NewState.Id,
                            Text = stateChange.NewState.Text
                        };

                        RaisePropertyChanged(nameof(MonitorInstanceViewModel.Instance));
                        EnableActions();
                        break;
                    default:
                        break;
                }
            }

            AddMessage(notification);
        }


        private void _hostWebSocket_MessageReceived(object sender, string json)
        {
            var notification = JsonConvert.DeserializeObject<LagoVista.Client.Core.Models.Notification>(json);

            if (!String.IsNullOrEmpty(notification.PayloadType))
            {
                switch (notification.PayloadType)
                {
                    case "StateChangeNotification":
                        var stateChange = JsonConvert.DeserializeObject<StateChangeNotification>(notification.Payload);
                        var titleCasedId = $"{stateChange.NewState.Id.Substring(0, 1).ToUpper()}{stateChange.NewState.Id.Substring(1)}";

                        Host.Status = new EntityHeader<HostStatus>()
                        {
                            Id = stateChange.NewState.Id,
                            Text = stateChange.NewState.Text
                        };

                        RaisePropertyChanged(nameof(MonitorInstanceViewModel.Host));
                        EnableActions();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                AddMessage(notification);
            }
        }

        public MonitorInstanceViewModel()
        {
            DeployHostCommand = new RelayCommand(() => SendAction(ResourceType.Instance, "deployhost", Resources.PlatformManagerResources.ServerAction_SentDeploy));
            RemoveServerCommand = new RelayCommand(() => SendAction(ResourceType.Instance, "destroyhost", Resources.PlatformManagerResources.ServerAction_SentRemove));
            RestartContainer = new RelayCommand(() => SendAction(ResourceType.Instance, "resetartcontainer", Resources.PlatformManagerResources.ServerAction_SentResetContainer));
            RestartHost = new RelayCommand(() => SendAction(ResourceType.Instance, "restarthost", Resources.PlatformManagerResources.ServerAction_SentReset));
            StartApplication = new RelayCommand(() => SendAction(ResourceType.Instance, "start", Resources.PlatformManagerResources.ServerAction_SentStart));
            PauseApplication = new RelayCommand(() => SendAction(ResourceType.Instance, "pause", Resources.PlatformManagerResources.ServerAction_SentPause));
            StopApplication = new RelayCommand(() => SendAction(ResourceType.Instance, "stop", Resources.PlatformManagerResources.ServerAction_SentStop));
            ReloadSolution = new RelayCommand(() => SendAction(ResourceType.Instance, "reloadsolution", Resources.PlatformManagerResources.ServerAction_SentReloadSolution));
            UpdateRuntime = new RelayCommand(() => SendAction(ResourceType.Instance, "updateruntime", Resources.PlatformManagerResources.ServerAction_SentUpdateRuntime));
            ShowInstanceTelemetryCommand = new RelayCommand(ShowInstanceTelemetry);
            ShowHostTelemetryCommand = new RelayCommand(ShowHostTelemetry);

            CanRemoveServer = false;
            CanRestartServer = false;
            CanRestartContainer = false;
            CanUpdateRuntime = false;
            CanDeployHost = false;
            CanStopApplication = false;
            CanPauseApplication = false;
            CanStartApplication = false;
            CanReloadSolution = false;
        }

        public async void ShowInstanceTelemetry()
        {
            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(TelemetryViewModel),
                ChildId = Instance.Id,               
            };

            launchArgs.Parameters.Add(TelemetryViewModel.VIEW_TYPE, TelemetryViewModel.VIEW_TYPE_INSTANCE);
            launchArgs.Parameters.Add(TelemetryViewModel.VIEW_NAME, Instance.Name);

            await ViewModelNavigation.NavigateAsync(launchArgs);
        }

        public async void ShowHostTelemetry()
        {
            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(TelemetryViewModel),
                ChildId = Instance.Id,
            };

            launchArgs.Parameters.Add(TelemetryViewModel.VIEW_TYPE, TelemetryViewModel.VIEW_TYPE_HOST);
            launchArgs.Parameters.Add(TelemetryViewModel.VIEW_NAME, Instance.Name);

            await ViewModelNavigation.NavigateAsync(launchArgs);
        }


        public RelayCommand DeployHostCommand { get; private set; }
        public RelayCommand RemoveServerCommand { get; private set; }
        public RelayCommand RestartContainer { get; private set; }
        public RelayCommand RestartHost { get; private set; }
        public RelayCommand StartApplication { get; private set; }
        public RelayCommand PauseApplication { get; private set; }
        public RelayCommand StopApplication { get; private set; }
        public RelayCommand ReloadSolution { get; private set; }
        public RelayCommand UpdateRuntime { get; private set; }

        public RelayCommand ShowHostTelemetryCommand { get; private set; }
        public RelayCommand ShowInstanceTelemetryCommand { get; private set; }

        public ObservableCollection<Client.Core.Models.Notification> MessagesFromServer { get; private set; } = new ObservableCollection<Client.Core.Models.Notification>();

        private bool _canRemoveServer;
        public bool CanRemoveServer
        {
            get { return _canRemoveServer; }
            set { Set(ref _canRemoveServer, value); }
        }

        private bool _canRestartServer;
        public bool CanRestartServer
        {
            get { return _canRestartServer; }
            set { Set(ref _canRestartServer, value); }
        }

        private bool _canRestartContainer;
        public bool CanRestartContainer
        {
            get { return _canRestartContainer; }
            set { Set(ref _canRestartContainer, value); }
        }

        private bool _canUpdateRuntime;
        public bool CanUpdateRuntime
        {
            get { return _canUpdateRuntime; }
            set { Set(ref _canUpdateRuntime, value); }
        }

        private bool _canDeployHost;
        public bool CanDeployHost
        {
            get { return _canDeployHost; }
            set { Set(ref _canDeployHost, value); }
        }

        private bool _canStopApplication;
        public bool CanStopApplication
        {
            get { return _canStopApplication; }
            set { Set(ref _canStopApplication, value); }
        }

        private bool _canPauseApplication;
        public bool CanPauseApplication
        {
            get { return _canPauseApplication; }
            set { Set(ref _canPauseApplication, value); }
        }

        private bool _canStartApplication;
        public bool CanStartApplication
        {
            get { return _canStartApplication; }
            set { Set(ref _canStartApplication, value); }
        }

        private bool _canReloadSolution;
        public bool CanReloadSolution
        {
            get { return _canReloadSolution; }
            set { Set(ref _canReloadSolution, value); }
        }

        private bool _showIsBusy;
        public bool ShowIsBusy
        {
            get { return _showIsBusy; }
            set { Set(ref _showIsBusy, value);  }
        }
    }
}
