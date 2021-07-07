using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Deployment.Admin.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PlatformManager.Core.ViewModels
{
    public class TelemetryViewModel : AppViewModelBase
    {
        public const string VIEW_TYPE = "viewtype";
        public const string VIEW_TYPE_INSTANCE = "instance";
        public const string VIEW_TYPE_HOST = "host";

        public const string VIEW_NAME = "itemName";

        public string ViewType { get; set; }
        public string Id { get; set; }

        public override Task InitAsync()
        {
            if (!LaunchArgs.HasParam(VIEW_TYPE)) throw new Exception("Must pass in VIEW_TYPE as parameter.");
            if (!LaunchArgs.HasParam(VIEW_NAME)) throw new Exception("Must pass in VIEW_TYPE as parameter.");

            Id = LaunchArgs.ChildId;
            ViewType = LaunchArgs.Parameters[VIEW_TYPE].ToString();
            ViewTitle = LaunchArgs.Parameters[VIEW_NAME].ToString();

            ShowStatusHistory();

            return base.InitAsync();
        }

        private void LoadData(string uri)
        {
            PerformNetworkOperation(async () =>
            {
                var response = await RestClient.GetAsync<ListResponse<TelemetryReportData>>(uri);
                if (response.Successful)
                {
                    if (response.Result.Successful)
                    {
                        StatusData = response.Result.Model;
                        return InvokeResult.Success;
                    }
                    else
                    {
                        return response.Result.ToInvokeResult();
                    }
                }
                else
                {
                    return response.ToInvokeResult();
                }
            });
        }

        public void ShowStatusHistory()
        {
            var statusHistoryUri = $"/api/deployment/{ViewType}/{Id}/statushistory";
            LoadData(statusHistoryUri);
        }

        public void ShowCustomEvents()
        {
            var uri = $"/api/telemetry/customEvents/{ViewType}/{Id}";
            LoadData(uri);
        }

        private string _viewTitle;
        public string ViewTitle
        {
            get { return _viewTitle; }
            set { Set(ref _viewTitle, value); }
        }

        public void ShowExceptions()
        {
            var telemetry = $"/api/telemetry/error/{ViewType}/{Id}";
        }

        IEnumerable<TelemetryReportData> _reportData;
        public IEnumerable<TelemetryReportData> ReportData
        {
            get { return _reportData; }
            set { Set(ref _reportData, value); }
        }

        IEnumerable<TelemetryReportData> _statusData;
        public IEnumerable<TelemetryReportData> StatusData
        {
            get { return _statusData; }
            set { Set(ref _statusData, value); }
        }
    }
}
