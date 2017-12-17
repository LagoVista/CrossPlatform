using LagoVista.Client.Core.Resources;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Models.Orgs;
using System.Collections.Generic;

namespace LagoVista.Client.Core.ViewModels.Orgs
{
    public class UserOrgsViewModel : ListViewModelBase<OrgUser>
    {

        IDeviceInfo _deviceInfo;
        IAppConfig _appConfig;
        public UserOrgsViewModel(IDeviceInfo deviceInfo, IAppConfig appConfig)
        {
            _deviceInfo = deviceInfo;
            _appConfig = appConfig;
        }

        protected override string GetListURI()
        {
            return $"/api/user/{AuthManager.User.Id}/orgs";
        }

        protected async override void ItemSelected(OrgUser model)
        {
            SelectedItem = null;
            if (model.OrgId == AuthManager.User.CurrentOrganization.Id)
            {
                return;
            }

            var result = await PerformNetworkOperation(async () =>
            {
                var authRequest = new AuthRequest()
                {
                    AppId = _appConfig.AppId,
                    ClientType = "mobileapp",
                    DeviceId = _deviceInfo.DeviceUniqueId,
                    AppInstanceId = AuthManager.AppInstanceId,
                    GrantType = "refreshtoken",
                    UserName = AuthManager.User.Email,
                    Email = AuthManager.User.Email,
                    RefreshToken = AuthManager.RefreshToken,
                    OrgId = model.OrgId,
                    OrgName = model.OrganizationName
                };
                var response = await RestClient.PostAsync<AuthRequest, AuthResponse>("/api/org/change", authRequest);
                if (!response.Successful) return response.ToInvokeResult();
                AuthManager.Roles = response.Result.Roles;

                var refreshResult = await RefreshUserFromServerAsync();
                if (refreshResult.Successful)
                {
                    await Popups.ShowAsync($"{ClientResources.UserOrgs_WelcometoNew} {AuthManager.User.CurrentOrganization.Text}");
                }

                return refreshResult.ToInvokeResult();
            });

            if(result.Successful)
            {
                await ViewModelNavigation.GoBackAsync();
            }
        }

        protected override void SetListItems(IEnumerable<OrgUser> items)
        {
            foreach (var org in items)
            {
                //HACK (MAJOR ONE, created https://slsys.visualstudio.com/LagoVista/_workitems?id=43&_a=edit to address this later)
                org.ETag = (org.OrgId == AuthManager.User.CurrentOrganization.Id).ToString();
            }

            ListItems = items;
        }

    }
}
