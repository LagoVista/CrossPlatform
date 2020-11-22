using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Client.Core.ViewModels.Users;
using LagoVista.XPlat.Core.Services;
using LagoVista.XPlat.Core.Views.Auth;
using LagoVista.XPlat.Core.Views.DeviceAccess;
using LagoVista.XPlat.Core.Views.Orgs;
using LagoVista.XPlat.Core.Views.Other;
using LagoVista.XPlat.Core.Views.Users;

namespace LagoVista.XPlat.Core
{
    public static class Startup
    {

        public static void Init(Xamarin.Forms.Application app, ViewModelNavigation nav)
        {
            nav.Add<LoginViewModel, LoginView>();
            nav.Add<ChangePasswordViewModel, ChangePasswordView>();
            nav.Add<SendResetPasswordLinkViewModel, SendResetPasswordView>();
            nav.Add<ResetPasswordViewModel, ResetPasswordView>();
            nav.Add<InviteUserViewModel, InviteUserView>();

            nav.Add<ConsoleViewModel, ConsoleView>();
            nav.Add<DeviceReposViewModel, DeviceReposView>();
            nav.Add<DeviceViewModel, DeviceView>();
            nav.Add<DevicesViewModel, DevicesView>();
            nav.Add<DFUViewModel, DFUView>();
            nav.Add<EditDeviceViewModel, EditDeviceView>();
            nav.Add<IOConfigViewModel, IOConfigView>();
            nav.Add<LiveDataStreamViewModel, LiveDataView>();
            nav.Add<LiveDataViewModel, LiveDataView>();
            nav.Add<LiveMessagesViewModel, LiveMessagesView>();
            nav.Add<PairBTDeviceViewModel, PairBTDeviceView>();
            nav.Add<ProvisionDeviceViewModel, ProvisionDevicewView>();

            nav.Add<RegisterUserViewModel, RegisterView>();
            nav.Add<VerifyUserViewModel, VerifyUserView>();
            nav.Add<OrgEditorViewModel, OrgEditorView>();
            nav.Add<UserOrgsViewModel, UserOrgsView>();
            nav.Add<AboutViewModel, AboutView>();
            nav.Add<AcceptInviteViewModel, AcceptInviteView>();
        }
    }
}
