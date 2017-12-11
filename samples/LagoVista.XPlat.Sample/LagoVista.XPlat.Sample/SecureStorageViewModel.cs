using System;
using System.Collections.Generic;
using System.Text;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Client.Core.Auth;

namespace LagoVista.XPlat.Sample
{
    public class SecureStorageViewModel : XPlatViewModel
    {

        public SecureStorageViewModel()
        {
            SetSecureValueCommand = new RelayCommand(SetSecureValue);
            GetSecureValueCommand = new RelayCommand(GetSecureValue);
            ResetStorageCommand = new RelayCommand(ResetStorage);
            PasswordToUnlock = "TEST1234";
        }

        public async void SetSecureValue()
        {
            if (!SecureStorage.IsUnlocked)
            {
                if (!SecureStorage.UnlockSecureStorage(PasswordToUnlock))
                {
                    await Popups.ShowAsync("COULD NOT UNLOCK STORAGE!");
                    return;
                }
            }

            SecureStorage.Store("SECUREVALUE", ValueToSave);
        }

        public void ResetStorage()
        {
            SecureStorage.Reset(PasswordToUnlock);
        }

        public async void GetSecureValue()
        {
            if (!SecureStorage.IsUnlocked)
            {
                if (!SecureStorage.UnlockSecureStorage(PasswordToUnlock))
                {
                    await Popups.ShowAsync("COULD NOT UNLOCK STORAGE!");
                    return;
                }
            }

            RestoredValue = SecureStorage.Retrieve("SECUREVALUE");
        }

        public string ValueToSave { get; set; }

        public string PasswordToUnlock { get; set; }

        private string _restoredValue;
        public string RestoredValue
        {
            get { return _restoredValue; }
            set { Set(ref _restoredValue, value); }
        }

        public RelayCommand SetSecureValueCommand { get; private set; }
        public RelayCommand GetSecureValueCommand { get; private set; }

        public RelayCommand ResetStorageCommand { get; private set; }

    }
}
