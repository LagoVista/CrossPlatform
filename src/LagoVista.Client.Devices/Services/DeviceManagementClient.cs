using LagoVista.Client.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LagoVista.Core.Validation;
using System;

namespace LagoVista.Client.Devices
{
    public class DeviceManagementClient : IDeviceManagementClient
    {

        LagoVista.Core.PlatformSupport.IStorageService _storageService;
        IRestClient _restClient;
        public DeviceManagementClient(IRestClient restClient, LagoVista.Core.PlatformSupport.IStorageService storageService)
        {
            _storageService = storageService;
            _restClient = restClient;
        }

        public Task<ListResponse<DeviceTypeSummary>> GetDeviceTypesAsync(ListRequest listRequest = null)
        {
            return _restClient.GetListResponseAsync<DeviceTypeSummary>("/api/devicetypes", listRequest);
        }

        public Task<ListResponse<DeviceConfigurationSummary>> GetDeviceConfigsAsync(ListRequest listRequest = null)
        {
            return _restClient.GetListResponseAsync<DeviceConfigurationSummary>("/api/deviceconfigs", listRequest);
        }

        public Task<ListResponse<DeviceRepositorySummary>> GetDeviceReposAsync(ListRequest listRequest = null)
        {
            return _restClient.GetListResponseAsync<DeviceRepositorySummary>("/api/devicerepos", listRequest);
        }

        public Task<InvokeResult> AddDeviceAsync(String deviceRepoId, Device device)
        {
            return _restClient.PostAsync($"/api/device/{deviceRepoId}", device);
        }

        public Task<InvokeResult> UpdateDeviceAsync(String deviceRepoId, Device device)
        {
            return _restClient.PutAsync($"/api/device/{deviceRepoId}", device);
        }

        public Task<InvokeResult<Device>> GetDeviceAsync(String deviceRepoId, String deviceId)
        {
            return _restClient.GetAsync<Device>(deviceId);
        }

        public Task<ListResponse<ClientAppSummary>> GetClientAppsAsync(ListRequest request = null)
        {
            return _restClient.GetListResponseAsync<ClientAppSummary>($"/api/clientapps", request);
        }

        public Task<InvokeResult<ClientApp>> GetClientAppAsync(String appId)
        {
            return _restClient.GetAsync<ClientApp>($"/api/clientapp/{appId}");
        }

        public Task<InvokeResult<DeploymentInstance>> GetDeploymentInstanceAsync(string instanceId)
        {
            return _restClient.GetAsync<DeploymentInstance>($"/api/deployment/instance/{instanceId}");
        }

        public async Task<ListResponse<DeviceSummary>> GetDevicesByDeviceTypeIdAsync(String appId)
        {
            var devices = new List<DeviceSummary>();

            var appResponse = await GetClientAppAsync(appId);
            if (!appResponse.Successful) return ListResponse<DeviceSummary>.FromError(appResponse.Errors.First().ToString());

            var instanceResponse = await GetDeploymentInstanceAsync(appResponse.Result.DeploymentInstance.Id);
            if (!instanceResponse.Successful) return ListResponse<DeviceSummary>.FromError(instanceResponse.Errors.First().ToString());
            if (EntityHeader.IsNullOrEmpty(instanceResponse.Result.DeviceRepository))
            {
                return ListResponse<DeviceSummary>.FromError("Application is not deployed or does not have a deployment repository.");
            }

            var repoId = instanceResponse.Result.DeviceRepository.Id;

            foreach (var config in appResponse.Result.DeviceConfigurations)
            {
                var configDevices = await GetDevicesByDeviceConfigIdAsync(repoId, config.Id);
                if (!configDevices.Successful) return configDevices;
                devices.AddRange(configDevices.Model);
            }

            foreach (var deviceType in appResponse.Result.DeviceConfigurations)
            {
                var deviceTypeDevices = await GetDevicesByDeviceTypeIdAsync(repoId, deviceType.Id);
                if (!deviceTypeDevices.Successful) return deviceTypeDevices;
                devices.AddRange(deviceTypeDevices.Model);
            }

            return ListResponse<DeviceSummary>.Create(devices);

        }

        public Task<ListResponse<DeviceSummary>> GetDevicesByDeviceTypeIdAsync(String deviceRepoId, String deviceTypeId, ListRequest request = null)
        {
            return _restClient.GetListResponseAsync<DeviceSummary>($"/api/devices/{deviceRepoId}/devicetype/{deviceTypeId}", request);
        }

        public Task<ListResponse<DeviceSummary>> GetDevicesByDeviceConfigIdAsync(String deviceRepoId, String deviceConfigId, ListRequest request = null)
        {
            return _restClient.GetListResponseAsync<DeviceSummary>($"/api/devices/{deviceRepoId}/devicetype/{deviceConfigId}", request);
        }

        public Task<ListResponse<DeploymentInstanceSummary>> GetDeploymentInstancesAsync(ListRequest listRequest = null)
        {
            return _restClient.GetListResponseAsync<DeploymentInstanceSummary>("/api/deployment/instances", listRequest);
        }
    }
}
