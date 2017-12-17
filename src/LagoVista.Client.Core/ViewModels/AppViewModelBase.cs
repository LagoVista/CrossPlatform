using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.PlatformSupport;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.Client.Core.Resources;
using LagoVista.Core.Validation;
using System.Threading;
using LagoVista.Client.Core.Exceptions;

namespace LagoVista.Client.Core.ViewModels
{
    public class AppViewModelBase : XPlatViewModel
    {
        protected HttpClient HttpClient { get { return SLWIOC.Get<HttpClient>(); } }
        protected IAuthManager AuthManager { get { return SLWIOC.Get<IAuthManager>(); } }
        protected ITokenManager TokenManager { get { return SLWIOC.Get<ITokenManager>(); } }
        protected IRestClient RestClient { get { return SLWIOC.Get<IRestClient>(); } }
        protected INetworkService NetworkService { get { return SLWIOC.Get<INetworkService>(); } }

        protected const string PASSWORD_REGEX = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
        protected const string EMAIL_REGEX = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";


        public async Task ShowServerErrorMessageAsync(InvokeResult result)
        {
            var bldr = new StringBuilder();
            foreach (var err in result.Errors)
            {
                bldr.AppendLine(err.Message);
            }

            await Popups.ShowAsync(bldr.ToString());
        }

        public async Task ShowServerErrorMessage<TResponseModel>(InvokeResult<TResponseModel> result)
        {
            var bldr = new StringBuilder();
            foreach (var err in result.Errors)
            {
                bldr.AppendLine(err.Message);
            }

            await Popups.ShowAsync(bldr.ToString());
        }

        public async Task ShowValidationErrorsAsync(ValidationResult result)
        {
            var bldr = new StringBuilder();
            foreach (var err in result.Errors)
            {
                bldr.AppendLine(err.Message);
            }

            await Popups.ShowAsync(bldr.ToString());
        }

        public async Task<InvokeResult> RefreshUserFromServerAsync()
        {
            var getUserResult = await RestClient.GetAsync("/api/user", new CancellationTokenSource());
            if (getUserResult.Success)
            {
                AuthManager.User = getUserResult.DeserializeContent<DetailResponse<UserInfo>>().Model;
                await AuthManager.PersistAsync();
            }

            return getUserResult.ToInvokeResult();
        }

        public async void Logout()
        {
            await AuthManager.LogoutAsync();
            await ViewModelNavigation.SetAsNewRootAsync<Auth.LoginViewModel>();
        }

        public async Task ForceLogoutAsync()
        {
            await Popups.ShowAsync(ClientResources.Err_PleaseLoginAgain);
            Logout();
        }

        public async Task<InvokeResult> PerformNetworkOperation(Func<Task<InvokeResult>> action, bool suppressErrorPopup = false)
        {
            if (!IsNetworkConnected)
            {
                await Popups.ShowAsync(ClientResources.Common_NoConnection);
                return InvokeResult.FromErrors(ClientResources.Common_NoConnection.ToErrorMessage());
            }

            IsBusy = true;

            InvokeResult result;
            try
            {
                result = await action();
                IsBusy = false;
                if (!suppressErrorPopup && !result.Successful)
                {                    
                    await ShowServerErrorMessageAsync(result);
                }
            }
            catch (CouldNotRenewTokenException)
            {
                IsBusy = false;
                await ForceLogoutAsync();
                return ClientResources.Err_CouldNotRenewToken.ToFailedInvokeResult();
            }
            catch (Exception ex)
            {
                IsBusy = false;
                Logger.AddException("IoTAppViewModelBase_PerformNetworkOperation", ex);
                await Popups.ShowAsync(ClientResources.Common_ErrorCommunicatingWithServer + "\r\n\r\n" + ex.Message);
                return ClientResources.Common_ErrorCommunicatingWithServer.ToFailedInvokeResult();
            }

            return result;
        }
    }
}
