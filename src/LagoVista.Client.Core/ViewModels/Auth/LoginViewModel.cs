using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Client.Core.ViewModels.Users;
using LagoVista.Core.Authentication.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.Auth
{
    public class LoginViewModel : AppViewModelBase
    {
        IAuthClient _authClient;
        IAppConfig _appConfig;
        IClientAppInfo _clientAppInfo;
        IDeviceInfo _deviceInfo;

        public LoginViewModel(IAuthClient authClient, IClientAppInfo clientAppInfo, IAppConfig appConfig, IDeviceInfo deviceInfo)
        {
            LoginCommand = new RelayCommand(LoginAsync);
            RegisterCommand = new RelayCommand(Register);
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);

            _authClient = authClient;
            _clientAppInfo = clientAppInfo;
            _appConfig = appConfig;
            _deviceInfo = deviceInfo;
        }

        public async Task<InvokeResult> PerformLoginAsync()
        {
            var loginInfo = new AuthRequest()
            {
                AppId = _appConfig.AppId,
                DeviceId = _deviceInfo.DeviceUniqueId,
                AppInstanceId = AuthManager.AppInstanceId,
                ClientType = "mobileapp",
                Email = EmailAddress,
                Password = Password,
                UserName = EmailAddress,
                GrantType = "password"
            };

            var loginResult = await _authClient.LoginAsync(loginInfo);
            if (!loginResult.Successful) return loginResult.ToInvokeResult();

            var authResult = loginResult.Result;
            AuthManager.AccessToken = authResult.AccessToken;
            AuthManager.AccessTokenExpirationUTC = authResult.AccessTokenExpiresUTC;
            AuthManager.RefreshToken = authResult.RefreshToken;
            AuthManager.AppInstanceId = authResult.AppInstanceId;
            AuthManager.RefreshTokenExpirationUTC = authResult.RefreshTokenExpiresUTC;
            AuthManager.IsAuthenticated = true;
            
            if (LaunchArgs.Parameters.ContainsKey("inviteid"))
            {
                var acceptInviteResult = await RestClient.GetAsync<InvokeResult>($"/api/org/inviteuser/accept/{LaunchArgs.Parameters["inviteId"]}");
                if (!acceptInviteResult.Successful) return acceptInviteResult.ToInvokeResult();
            }

            var refreshUserResult = await RefreshUserFromServerAsync();
            if (!refreshUserResult.Successful) return refreshUserResult;

            return InvokeResult.Success;
        }

        public async void Register()
        {
            await ViewModelNavigation.NavigateAsync<RegisterUserViewModel>(this);
        }

        public async void ForgotPassword()
        {
            await ViewModelNavigation.NavigateAsync<SendResetPasswordLinkViewModel>(this);
        }

        public async void LoginAsync()
        {
            var loginResult = await PerformNetworkOperation(PerformLoginAsync);
            if(loginResult.Successful)
            {
                if (AuthManager.User.EmailConfirmed && AuthManager.User.PhoneNumberConfirmed)
                {
                    // If no org, have them add an org....
                    if (EntityHeader.IsNullOrEmpty(AuthManager.User.CurrentOrganization))
                    {
                        await ViewModelNavigation.SetAsNewRootAsync<OrgEditorViewModel>();
                    }
                    else
                    {
                        // We are good, so show main screen.
                        await ViewModelNavigation.SetAsNewRootAsync(_clientAppInfo.MainViewModel);
                    }
                }
                else
                {
                    // Show verify user screen.
                    await ViewModelNavigation.SetAsNewRootAsync<VerifyUserViewModel>();
                }
            }
        }

        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand RegisterCommand { get; private set; }
        public RelayCommand ForgotPasswordCommand { get; private set; }

        private string _emailAddress;
        private string _password;

        public string EmailAddress
        {
            get { return _emailAddress; }
            set { Set(ref _emailAddress, value); }
        }

        public string Password
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }
    }
}