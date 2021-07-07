using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.Deployment.Admin.Models;
using System.Threading.Tasks;

namespace LagoVista.PlatformManager.Core.ViewModels
{
    public class InstanceViewModel : AppViewModelBase
    {
        public InstanceViewModel()
        {
            /* for now we just show a readonly view */
            InstanceTapCommand = new RelayCommand(InstanceTapped);
            ListenerTapCommand = new RelayCommand(ListenerTapped);
            PlannerTapCommand = new RelayCommand(PlannerTapped);
            MessageTypeTapCommand = new RelayCommand(MessageTypeTapped);
            PipelineModuleTapCommand = new RelayCommand(PipelineModuleTapped);
            ManageCommand = new RelayCommand(InstanceTapped);
        }

        

        public void InstanceTapped(object id)
        {
            NavigateAndViewAsync<MonitorInstanceViewModel>(LaunchArgs.ChildId);
        }

        public void ListenerTapped(object id)
        {
            NavigateAndViewAsync<ListenerViewModel>(id.ToString());
        }

        public void PlannerTapped(object id)
        {

        }

        public void MessageTypeTapped(object id)
        {

        }

        public void PipelineModuleTapped(object id)
        {
            NavigateAndViewAsync<PipelineViewModel>(id.ToString());
        }


        public async override Task InitAsync()
        {
            await PerformNetworkOperation(async () =>
            {
                var result = await RestClient.GetAsync<InvokeResult<InstanceRuntimeDetails>>($"/api/deployment/instance/{LaunchArgs.ChildId}/runtime");
                if(result.Successful)
                {
                    if (result.Result.Successful)
                    {
                        RuntimeDetails = result.Result.Result;
                    }
                    else
                    {
                        return result.Result.ToInvokeResult();
                    }
                }
                return result.ToInvokeResult();
            });
        }

        InstanceRuntimeDetails _runtimeDetails;
        public InstanceRuntimeDetails RuntimeDetails
        {
            get { return _runtimeDetails; }
            set { Set(ref _runtimeDetails, value); }
        }

        public RelayCommand InstanceTapCommand { get; private set; }
        public RelayCommand ListenerTapCommand { get; private set; }
        public RelayCommand PlannerTapCommand { get; private set; }
        public RelayCommand MessageTypeTapCommand { get; private set; }
        public RelayCommand PipelineModuleTapCommand { get; private set; }

        public RelayCommand ManageCommand { get; private set; }
    }
}
