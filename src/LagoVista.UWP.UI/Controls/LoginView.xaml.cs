using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Authentication.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LagoVista.UWP.UI.Controls
{
    public sealed partial class LoginView : UserControl
    {
        public LoginView()
        {
            this.InitializeComponent();
            DataContext = new AuthViewModel();
        }
    }

    public class AuthViewModel : AppViewModelBase
    {
        public AuthViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            LogoutCommand = new RelayCommand(Logout);
        }

        public async override Task InitAsync()
        {
            await AuthManager.LoadAsync();
            RaisePropertyChanged(nameof(IAuthManager.IsAuthenticated));
            await base.InitAsync();
        }

        public void Login()
        {
            PerformNetworkOperation(async () =>
            {
                var appConfig = SLWIOC.Get<IAppConfig>();
                var authClient = SLWIOC.Get<IAuthClient>();
                var deviceInfo = SLWIOC.Get<IDeviceInfo>();

                var loginInfo = new AuthRequest()
                {
                    AppId = appConfig.AppId,
                    DeviceId = deviceInfo.DeviceUniqueId,
                    AppInstanceId = AuthManager.AppInstanceId,
                    ClientType = "mobileapp",
                    Email = Email,
                    Password = Password,
                    UserName = Email,
                    GrantType = "password"
                };

                var loginResult = await authClient.LoginAsync(loginInfo);
                if (loginResult.Successful)
                {
                    var authResult = loginResult.Result;
                    AuthManager.AccessToken = authResult.AccessToken;
                    AuthManager.AccessTokenExpirationUTC = authResult.AccessTokenExpiresUTC;
                    AuthManager.RefreshToken = authResult.RefreshToken;
                    AuthManager.AppInstanceId = authResult.AppInstanceId;
                    AuthManager.RefreshTokenExpirationUTC = authResult.RefreshTokenExpiresUTC;
                    AuthManager.IsAuthenticated = true;

                    var refreshUserResult = await RefreshUserFromServerAsync();

                    RaisePropertyChanged(nameof(IAuthManager.IsAuthenticated));

                    Password = string.Empty;
                }
                else
                {
                    await ShowServerErrorMessageAsync(loginResult.ToInvokeResult());
                }
            });
        }

        public async new void Logout()
        {
            await AuthManager.LogoutAsync();
            RaisePropertyChanged(nameof(IAuthManager.IsAuthenticated));
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { Set(ref _email, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }

        public bool IsAuthenticated
        {
            get { return AuthManager.IsAuthenticated; }
        }

        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
    }
}
