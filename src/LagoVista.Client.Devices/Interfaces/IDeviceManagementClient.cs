using System;
using System.Threading.Tasks;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;

namespace LagoVista.Client.Devices
{
    public interface IDeviceManagementClient
    {
        Task<InvokeResult> AddDeviceAsync(string deviceRepoId, Device device);
        Task<Device> GetDeviceAsync(string deviceRepoId, string deviceId);
        Task<ListResponse<DeviceConfigurationSummary>> GetDeviceConfigsAsync(ListRequest listRequest = null);
        Task<ListResponse<DeviceRepositorySummary>> GetDeviceReposAsync(ListRequest listRequest = null);
        Task<ListResponse<DeploymentInstanceSummary>> GetDeploymentInstancesAsync(ListRequest listRequest = null);
        Task<ListResponse<ClientAppSummary>> GetClientAppsAsync(ListRequest request = null);
        Task<ListResponse<DeviceSummary>> GetDevicesByDeviceTypeIdAsync(String appId);
        Task<DeploymentInstance> GetDeploymentInstanceAsync(string instanceId);
        Task<ClientApp> GetClientAppAsync(String appId);
        Task<ListResponse<DeviceSummary>> GetDevicesByDeviceConfigIdAsync(string instanceId, string deviceConfig, ListRequest request = null);
        Task<ListResponse<DeviceSummary>> GetDevicesByDeviceTypeIdAsync(string deviceRepoId, string appDeviceTypeId, ListRequest request = null);
    
        Task<ListResponse<DeviceSummary>> GetChildDevicesAsync(string deviceRepoId, string deviceId, ListRequest request = null);
        Task<InvokeResult> AttachChildDeviceAsync(string deviceRepoId, string parentDeviceId, string chidlDeviceId);
        Task<InvokeResult> RemoveChildDevice(string deviceRepoId, string parentDeviceId, string chidlDeviceId);

        Task<ListResponse<DeviceTypeSummary>> GetDeviceTypesAsync(ListRequest listRequest = null);
        Task<InvokeResult> UpdateDeviceAsync(string deviceRepoId, Device device);
    }
}