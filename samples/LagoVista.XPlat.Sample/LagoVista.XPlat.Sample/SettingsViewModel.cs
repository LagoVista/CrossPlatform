using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample
{
    public class SettingsViewModel : XPlatViewModel
    {
        public SettingsViewModel()
        {
            AddSettingCommand = new RelayCommand(AddSetting);
            HasSettingCommand = new RelayCommand(HasSetting);
            GetSettingCommand = new RelayCommand(GetSetting);
            RemoveSettingCommand = new RelayCommand(RemoveSetting);
        }

        public async void AddSetting(object obj)
        {
            await Storage.StoreKVP("SETTING1", new SampleSetting()
            {
                DateStamp = DateTime.Now,
                Value1 = "ABC123",
                Value2 = 10023
            });
        }

        public async void HasSetting(object obj)
        {
            await Popups.ShowAsync((await Storage.HasKVPAsync("SETTING1")).ToString());
        }

        public async void RemoveSetting(object obj)
        {
            await Storage.ClearKVP("SETTING1");
        }

        public async void GetSetting(object obj)
        {
            var setting = await Storage.GetKVPAsync<SampleSetting>("SETTING1");

            if (setting == null)
                await Popups.ShowAsync("Setting Empty");
            else
                await Popups.ShowAsync(setting.Value1 + " " + setting.Value2 + " " + setting.DateStamp.ToString());
        }


        public RelayCommand AddSettingCommand { get; }
        public RelayCommand HasSettingCommand { get; }
        public RelayCommand GetSettingCommand { get; }
        public RelayCommand RemoveSettingCommand { get; }
    }

    public class SampleSetting
    {
        public string Value1 { get; set; }

        public int Value2 { get; set; }

        public DateTime DateStamp { get; set; }
    }
}
