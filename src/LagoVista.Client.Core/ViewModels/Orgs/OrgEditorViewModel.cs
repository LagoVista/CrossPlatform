using LagoVista.Core.Commanding;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.ViewModels.Organization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.Orgs
{
    public class OrgEditorViewModel : FormViewModelBase<CreateOrganizationViewModel>
    {
        IClientAppInfo _clientAppInfo;

        public OrgEditorViewModel(IClientAppInfo clientAppInfo)
        {
            _clientAppInfo = clientAppInfo;

            LogoutCommand = new RelayCommand(Logout);

            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    Command = LogoutCommand,
                    Name = "Logout",
                    FontIconKey = "fa-sign-out"
                }
            };
        }

        public override Task PostSaveAsync()
        {
            return ViewModelNavigation.SetAsNewRootAsync(_clientAppInfo.MainViewModel);
        }        

        protected override string GetRequestUri()
        {
            return "/api/org/factory";
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Namespace));
            form.AddViewCell(nameof(Model.WebSite));
        }

        public override Task<InvokeResult> SaveRecordAsync()
        {
            return PerformNetworkOperation(async () =>
            {
                var saveResult = await FormRestClient.AddAsync("/api/org", this.Model);
                if (!saveResult.Successful) return saveResult;

                var refreshResult = await RefreshUserFromServerAsync();
                return refreshResult;
            });
        }

        public RelayCommand LogoutCommand { get; private set; }
    }
}