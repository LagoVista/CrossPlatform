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
        IClientAppInfo _clientAppInfo;
        IDeviceInfo _deviceInfo;

        public LoginViewModel(IAuthClient authClient, IClientAppInfo clientAppInfo, IAppConfig appConfig, IDeviceInfo deviceInfo)
        {
            LoginCommand = new RelayCommand(LoginAsync);
            RegisterCommand = new RelayCommand(Register);
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);

            _authClient = authClient;
            _clientAppInfo = clientAppInfo;
            AppConfig = appConfig;
            _deviceInfo = deviceInfo;
        }

        public IAppConfig AppConfig { get; }

        public async Task<InvokeResult> PerformLoginAsync()
        {
            var loginInfo = new AuthRequest()
            {
                AuthType =AppConfig.AuthType,
                DeviceRepoId = AppConfig.DeviceRepoId,
                AppId = AppConfig.AppId,
                DeviceId = _deviceInfo.DeviceUniqueId,
                AppInstanceId = AuthManager.AppInstanceId,
                ClientType = AppConfig.ClientType,
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

        public void Register()
        {
            switch(AppConfig.Environment)
            {
                case Environments.Production:
                case Environments.Beta: Services.Network.OpenURI(new System.Uri("https://www.nuviot.com/account/register")); break;

                case Environments.Local: 
                case Environments.LocalDevelopment: Services.Network.OpenURI(new System.Uri("http://localhost:5000/account/register")); break;

                case Environments.Development: Services.Network.OpenURI(new System.Uri("https://dev.nuviot.com/account/register")); break;

                case Environments.Staging:
                case Environments.Testing: Services.Network.OpenURI(new System.Uri("https://test.nuviot.com/account/register")); break;
            }
        }

        public  void ForgotPassword()
        {
            switch (AppConfig.Environment)
            {
                case Environments.Production:
                case Environments.Beta: Services.Network.OpenURI(new System.Uri("https://www.nuviot.com/account/forgotpassword")); break;

                case Environments.Local:
                case Environments.LocalDevelopment: Services.Network.OpenURI(new System.Uri("http://localhost:5000/account/forgotpassword")); break;

                case Environments.Development: Services.Network.OpenURI(new System.Uri("https://dev.nuviot.com/account/forgotpassword")); break;

                case Environments.Staging:
                case Environments.Testing: Services.Network.OpenURI(new System.Uri("https://test.nuviot.com/account/forgotpassword")); break;
            }
        }

        public async void LoginAsync()
        {
            var loginResult = await PerformNetworkOperation(PerformLoginAsync);
            if(loginResult.Successful)
            {
                if (AuthManager.User.EmailConfirmed && AuthManager.User.PhoneNumberConfirmed &&
                    !EntityHeader.IsNullOrEmpty(AuthManager.User.CurrentOrganization)) 
                {
                    await ViewModelNavigation.SetAsNewRootAsync(_clientAppInfo.MainViewModel);

                    // If no org, have them add an org....
                    /*                    if (EntityHeader.IsNullOrEmpty(AuthManager.User.CurrentOrganization))
                                        {
                                            await ViewModelNavigation.SetAsNewRootAsync<OrgEditorViewModel>();
                                        }
                                        else*
                                        {
                                            // We are good, so show main screen.
                                            await ViewModelNavigation.SetAsNewRootAsync(_clientAppInfo.MainViewModel);
                                        }*/
                }
                else
                {
                    await Popups.ShowAsync("Please complete the setup process on the NuvIoT web site before logging in to the mobile  application.");
                    // Show verify user screen.
                    //await ViewModelNavigation.SetAsNewRootAsync<VerifyUserViewModel>();
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