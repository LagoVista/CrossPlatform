using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.Forms
{
    public class FormViewModel<TEntity> : AppViewModelBase where TEntity: class, new()
    {
        DetailResponse<TEntity> _form;
        public DetailResponse<TEntity> Form
        {
            get => _form;
            set => Set(ref _form, value);
        }

        ObservableCollection<FormField> _fields;
        public ObservableCollection<FormField> Fields
        {
            get => _fields;
            set => Set(ref _fields, value);
        }

        public override async Task InitAsync()
        {
            var url = LaunchArgs.Parameters["formurl"].ToString();

            var response = await RestClient.GetAsync<DetailResponse<TEntity>>(url);
            if (response.Successful)
            {
                Form = response.Result;
                var fields = new ObservableCollection<FormField>();
                foreach(var field in Form.FormFields)
                {
                    fields.Add(Form.View[field]);
                }

                Fields = fields;
            }
        }
    }
}
