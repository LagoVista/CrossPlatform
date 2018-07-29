﻿using System;
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
        Task<InvokeResult<Device>> GetDeviceAsync(string deviceRepoId, string deviceId);
        Task<ListResponse<DeviceConfigurationSummary>> GetDeviceConfigsAsync(ListRequest listRequest = null);
        Task<ListResponse<DeviceRepositorySummary>> GetDeviceReposAsync(ListRequest listRequest = null);
        Task<ListResponse<DeploymentInstanceSummary>> GetDeploymentInstancesAsync(ListRequest listRequest = null);
        Task<ListResponse<ClientAppSummary>> GetClientAppsAsync(ListRequest request = null);
        Task<ListResponse<DeviceSummary>> GetDevicesByDeviceTypeIdAsync(String appId);
        Task<InvokeResult<DeploymentInstance>> GetDeploymentInstanceAsync(string instanceId);
        Task<InvokeResult<ClientApp>> GetClientAppAsync(String appId);
        Task<ListResponse<DeviceSummary>> GetDevicesByDeviceConfigIdAsync(string instanceId, string deviceConfig, ListRequest request = null);
        Task<ListResponse<DeviceSummary>> GetDevicesByDeviceTypeIdAsync(string deviceRepoId, string appDeviceTypeId, ListRequest request = null);
        Task<ListResponse<DeviceTypeSummary>> GetDeviceTypesAsync(ListRequest listRequest = null);
        Task<InvokeResult> UpdateDeviceAsync(string deviceRepoId, Device device);
    }
}