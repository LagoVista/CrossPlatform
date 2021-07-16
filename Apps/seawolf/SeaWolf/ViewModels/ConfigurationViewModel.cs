using LagoVista.Client.Core.Net;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Attributes;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class ConfigurationViewModel : AppViewModelBase
    {

        string _deviceRepoId;
        string _deviceId;

        Device _device;
        FormRestClient<Device> _formCLient;

        public ConfigurationViewModel()
        {
            _formCLient = new FormRestClient<Device>(base.RestClient);
        }

        public override Task InitAsync()
        {
            PerformNetworkOperation(async () =>
            {
                var result = await _formCLient.GetAsync(GetRequestUri());

                _device = result.Result.Model;

                var form = new EditFormAdapter(result.Result.Model, result.Result.View, ViewModelNavigation);

                form.AddViewCell(nameof(Device.Name));

                ShowProperties(form, result.Result.Model);

                form.FormItems.Where(itm => itm.Name == nameof(Device.Name).ToLower()).First().Value = result.Result.Model.Name;

                FormAdapter = form;
            });

            return base.InitAsync();
        }

        EditFormAdapter _editFormAdapter;
        public EditFormAdapter FormAdapter
        {
            get => _editFormAdapter;
            set => Set(ref _editFormAdapter, value);
        }

        protected string GetRequestUri()
        {
            _deviceRepoId = LaunchArgs.Parameters[ComponentViewModel.DeviceRepoId].ToString();
            _deviceId = LaunchArgs.Parameters[ComponentViewModel.DeviceId].ToString();

            return $"/api/device/{_deviceRepoId}/{_deviceId}/metadata";
        }


        private void ShowProperties(EditFormAdapter formAdapter, Device _device)
        {
            foreach (var field in _device.PropertiesMetaData)
            {
                var formField = FormField.Create(field.Key, new FormFieldAttribute(), null);

                formField.Label = field.Label;

                switch (field.FieldType.Value)
                {
                    case ParameterTypes.State:
                        formField.Options = new List<EnumDescription>();
                        foreach (var state in field.StateSet.Value.States)
                        {
                            formField.Options.Add(new EnumDescription() { Key = state.Key, Label = state.Name, Name = state.Name });
                        }

                        formField.FieldType = FormField.FieldType_Picker;

                        var initialState = field.StateSet.Value.States.Where(st => st.IsInitialState).FirstOrDefault();
                        if (initialState != null)
                        {
                            formField.Value = initialState.Key;
                        }

                        break;
                    case ParameterTypes.String:
                        formField.FieldType = FormField.FieldType_Text;
                        formField.Value = field.DefaultValue;
                        break;
                    case ParameterTypes.DateTime:
                        formField.FieldType = FormField.FieldType_DateTime;
                        formField.Value = field.DefaultValue;
                        break;
                    case ParameterTypes.Decimal:
                        formField.FieldType = FormField.FieldType_Decimal;
                        formField.Value = field.DefaultValue;
                        break;
                    case ParameterTypes.GeoLocation:
                        formField.FieldType = FormField.FieldType_Text;
                        formField.Value = field.DefaultValue;
                        break;
                    case ParameterTypes.TrueFalse:
                        formField.FieldType = FormField.FieldType_CheckBox;
                        formField.Value = field.DefaultValue;
                        break;
                    case ParameterTypes.ValueWithUnit:
                        formField.FieldType = FormField.FieldType_Decimal;
                        formField.Value = field.DefaultValue;
                        break;
                }

                formField.IsRequired = field.IsRequired;
                formField.IsUserEditable = !field.IsReadOnly;

                var fieldValue = _device.Properties.Where(prop => prop.Key == field.Key).FirstOrDefault();

                formField.Value = fieldValue.Value;

                //formAdapter.V
                formAdapter.FormItems.Add(formField);

                // formAdapter.AddViewCell(field.Key.Substring(0,1).ToUpper() + field.Key.Substring(1));
            }
        }


        public async override void Save()
        {
            foreach (var formItem in FormAdapter.FormItems)
            {
                var property = _device.Properties.Where(prop => prop.Key == formItem.Name).FirstOrDefault();
                if (property != null)
                    property.Value = formItem.Value;
            }

            var result = await PerformNetworkOperation(() =>
            {
                return _formCLient.UpdateAsync($"/api/device/{_deviceRepoId}", _device);
            });

            if (result.Successful)
            {
                this.CloseScreen();
            }
        }
    }
}
