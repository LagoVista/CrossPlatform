using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using System;
using System.Threading;
using Newtonsoft.Json;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.Client.Core.Resources;
using System.Collections.Generic;
using LagoVista.Client.Core.ViewModels.Orgs;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.Users
{
    public class VerifyUserViewModel : AppViewModelBase
    {
        IClientAppInfo _clientAppInfo;

        public VerifyUserViewModel(IClientAppInfo clientAppInfo)
        {
            _clientAppInfo = clientAppInfo;
            SendEmailConfirmationCommand = new RelayCommand(SendEmailConfirmation);
            SendSMSConfirmationCommand = new RelayCommand(SendSMSConfirmation, ValidPhoneNumber);
            ConfirmEnteredSMSCommand = new RelayCommand(ConfirmSMSCode, () => !String.IsNullOrEmpty(SMSCode));
            LogoutCommand = new RelayCommand(Logout);
        }

        public void SendEmailConfirmation()
        {
            PerformNetworkOperation(async () =>
            {
                var result = await RestClient.GetAsync("/api/verify/sendconfirmationemail", new CancellationTokenSource());
                if (result.Success)
                {
                    if (result.ToInvokeResult().Successful)
                    {
                        await Popups.ShowAsync(ClientResources.Verify_EmailSent);
                    }
                    else
                    {
                        await ShowServerErrorMessageAsync(result.ToInvokeResult());
                    }
                }
                else
                {
                    await Popups.ShowAsync(result.ErrorMessage);
                }
            });
        }

        public override Task InitAsync()
        {
            if(AuthManager.User.PhoneNumberConfirmed)
            {
                ShowPhoneConfirm = false;
                ShowEmailConfirm = true;
            }
            else
            {
                ShowEmailConfirm = false;
                ShowPhoneConfirm = true;
            }

            return base.InitAsync();
        }

        public bool ValidPhoneNumber()
        {
            return !(String.IsNullOrEmpty(PhoneNumber));
        }

        public async Task TransitionToNextView()
        {
            if (AuthManager.User.EmailConfirmed && AuthManager.User.PhoneNumberConfirmed)
            {
                if (EntityHeader.IsNullOrEmpty(AuthManager.User.CurrentOrganization))
                {
                    await ViewModelNavigation.SetAsNewRootAsync<OrgEditorViewModel>();
                }
                else
                {
                    await ViewModelNavigation.SetAsNewRootAsync(_clientAppInfo.MainViewModel);
                }
            }
        }

        public async void ConfirmSMSCode()
        {
            var result = await PerformNetworkOperation(async () =>
            {
                var vm = new VerfiyPhoneNumber();
                vm.PhoneNumber = PhoneNumber;
                vm.SMSCode = SMSCode;
                var confirmSMSResult = await RestClient.PostAsync("/api/verify/sms", vm, new CancellationTokenSource());
                if (!confirmSMSResult.Successful) return confirmSMSResult;

                var refreshResult = await RefreshUserFromServerAsync();
                if (!refreshResult.Successful) return refreshResult;

                Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Error, "VerifyUserViewModel_ConfirmSMSCode", "SMS Confirmed", new KeyValuePair<string, string>("userid", AuthManager.User.Id));
                await Popups.ShowAsync(ClientResources.Verify_SMS_Confirmed);

                ShowPhoneConfirm = false;
                ShowEmailConfirm = true;

                return refreshResult;
            });

            if (result.Successful)
            {
                await TransitionToNextView();
            }
        }

        public override async void HandleURIActivation(Uri uri, Dictionary<string, string> kvps)
        {
            if (!kvps.ContainsKey("code"))
            {
                Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Error, "VerifyUserViewModel_HandleURIActivation", "Missing Code", new KeyValuePair<string, string>("queryString", uri.Query));
                return;
            }

            if (!kvps.ContainsKey("userid"))
            {
                Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Error, "VerifyUserViewModel_HandleURIActivation", "Missing User ID", new KeyValuePair<string, string>("queryString", uri.Query));
                return;
            }

            var code = kvps["code"];
            var userId = kvps["userid"];

            if (userId != AuthManager.User.Id)
            {
                Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Error, "VerifyUserViewModel_HandleURIActivation", "Link/User Id Mismatch",
                    new KeyValuePair<string, string>("linkUser", userId),
                     new KeyValuePair<string, string>("currentUser", AuthManager.User.Id));
                return;
            }

            var result = await PerformNetworkOperation(async () =>
            {
                var vm = new ConfirmEmail();
                vm.ReceivedCode = System.Net.WebUtility.UrlDecode(code);
                var json = JsonConvert.SerializeObject(vm);
                var confirmEmailResult = await RestClient.PostAsync("/api/verify/email", vm, new CancellationTokenSource());
                if (!confirmEmailResult.Successful) return confirmEmailResult;

                var refreshResult = await RefreshUserFromServerAsync();
                if (!refreshResult.Successful) return refreshResult;

                Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Error, "VerifyUserViewModel_HandleURIActivation", "EmailConfirmed", new KeyValuePair<string, string>("userid", userId));
                await Popups.ShowAsync(ClientResources.Verify_Email_Confirmed);

                await RefreshUserFromServerAsync();

                return refreshResult;
            });

            if (result.Successful)
            {
                await TransitionToNextView();
            }
        }

        public async void SendSMSConfirmation()
        {
            var result = await PerformNetworkOperation(async () =>
            {
                var vm = new VerfiyPhoneNumber();
                vm.PhoneNumber = PhoneNumber;
                var sendMSM = await RestClient.PostAsync("/api/verify/sendsmscode", vm, new CancellationTokenSource());
                return sendMSM;
            });

            if (result.Successful)
            {
                await Popups.ShowAsync(ClientResources.Verify_SMSSent);
            }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                Set(ref _phoneNumber, value);
                SendSMSConfirmationCommand.RaiseCanExecuteChanged();
            }
        }

        private string _smsCode;
        public string SMSCode
        {
            get { return _smsCode; }
            set
            {
                Set(ref _smsCode, value);
                ConfirmEnteredSMSCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _confirmEmailStepVisible = false;
        public bool ConfirmEmailStepVisible
        {
            get { return _confirmEmailStepVisible; }
            set { Set(ref _confirmEmailStepVisible, value); }
        }

        private bool _showPhoneConfirm = false;
        public bool ShowPhoneConfirm
        {
            get { return _showPhoneConfirm; }
            set
            {
                _showPhoneConfirm = value;
                RaisePropertyChanged();
            }
        }

        private bool _showEmailConfirm = false;
        public bool ShowEmailConfirm
        {
            get { return _showEmailConfirm; }
            set
            {
                _showEmailConfirm = value;
                RaisePropertyChanged();
            }
        }


        public RelayCommand SendEmailConfirmationCommand { get; private set; }
        public RelayCommand SendSMSConfirmationCommand { get; private set; }
        public RelayCommand ConfirmEnteredSMSCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
    }
}