using LagoVista.Client.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Device> GetDeviceAsync(String deviceRepoId, String deviceId)
        {
            var result = await _restClient.GetAsync<DetailResponse<Device>>($"/api/device/{deviceRepoId}/{deviceId}");
            if (result.Successful)
            {
                return result.Result.Model;
            }
            else
            {
                throw new Exception(result.Errors.First().Message);
            }
        }

        public Task<ListResponse<ClientAppSummary>> GetClientAppsAsync(ListRequest request = null)
        {
            return _restClient.GetListResponseAsync<ClientAppSummary>($"/api/clientapps", request);
        }

        public async Task<ClientApp> GetClientAppAsync(String appId)
        {
            var result = await _restClient.GetAsync<DetailResponse<ClientApp>>($"/api/clientapp/{appId}");
            if (result.Successful)
            {
                return result.Result.Model;
            }
            else
            {
                throw new Exception(result.Errors.First().Message);
            }
        }

        public async Task<DeploymentInstance> GetDeploymentInstanceAsync(string instanceId)
        {
            var result = await _restClient.GetAsync<DetailResponse<DeploymentInstance>>($"/api/deployment/instance/{instanceId}");
            if (result.Successful)
            {
                return result.Result.Model;
            }
            else
            {
                throw new Exception(result.Errors.First().Message);
            }
        }

        public async Task<ListResponse<Models.DataStreamResult>> GetDataStreamValues(string dataStreamId, string deviceId)
        {
            var result = await _restClient.GetAsync<ListResponse<Models.DataStreamResult>>($"/api/datastream/{dataStreamId}/data/{deviceId}");
            if (result.Successful)
            {
                return result.Result;
            }
            else
            {
                throw new Exception(result.Errors.First().Message);
            }
        }

        public async Task<ListResponse<DeviceSummary>> GetDevicesByDeviceTypeIdAsync(String appId)
        {
            var devices = new List<DeviceSummary>();

            var clientApp = await GetClientAppAsync(appId);

            var instance = await GetDeploymentInstanceAsync(clientApp.DeploymentInstance.Id);

            if (EntityHeader.IsNullOrEmpty(instance.DeviceRepository))
            {
                return ListResponse<DeviceSummary>.FromError("Application is not deployed or does not have a deployment repository.");
            }

            var repoId = instance.DeviceRepository.Id;

            foreach (var config in clientApp.DeviceConfigurations)
            {
                var configDevices = await GetDevicesByDeviceConfigIdAsync(repoId, config.Id);
                if (!configDevices.Successful)
                {
                    return configDevices;
                }

                devices.AddRange(configDevices.Model);
            }

            foreach (var deviceType in clientApp.DeviceConfigurations)
            {
                var deviceTypeDevices = await GetDevicesByDeviceTypeIdAsync(repoId, deviceType.Id);
                if (!deviceTypeDevices.Successful)
                {
                    return deviceTypeDevices;
                }

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

        public Task<ListResponse<DeviceSummary>> GetChildDevicesAsync(string deviceRepoId, string deviceId, ListRequest request = null)
        {
            return _restClient.GetListResponseAsync<DeviceSummary>($"/api/devices/{deviceRepoId}/{deviceId}/children", request);
        }

        public async Task<InvokeResult> AttachChildDeviceAsync(string deviceRepoId, string parentDeviceId, string chidlDeviceId)
        {
            var result = await _restClient.GetAsync<InvokeResult>($"/api/devices/{parentDeviceId}/{parentDeviceId}/attachchild/{chidlDeviceId}");
            return result.Successful ? result.Result : result.ToInvokeResult();
        }

        public async Task<InvokeResult> RemoveChildDevice(string deviceRepoId, string parentDeviceId, string chidlDeviceId)
        {
            var result = await _restClient.GetAsync<InvokeResult>($"/api/devices/{parentDeviceId}/{parentDeviceId}/removechild/{chidlDeviceId}");
            return result.Successful ? result.Result : result.ToInvokeResult();
        }
    }
}
