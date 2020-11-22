using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample.ViewModels
{
    public class HomeViewModel : AppViewModelBase
    {

        public HomeViewModel()
        {
            MenuOptions = new List<MenuListItem>()
            {
                new MenuListItem() { Title = "Switch Orgs", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<UserOrgsViewModel>(this)) },
                new MenuListItem() { Title ="Logout", Command = new RelayCommand(() => Logout())},
                new MenuListItem() { Title = "Device Repos", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<DeviceReposViewModel>(this)) },
            };
        }

        public class MenuListItem
        {
            public RelayCommand Command { get; set; }
            public string Title { get; set; }
        }

        public List<MenuListItem> MenuOptions {get;}

        MenuListItem _selectedMenuOption;
        public MenuListItem SelectedMenuOption
        {
            get { return _selectedMenuOption; }
            set
            {
                _selectedMenuOption = null;
                RaisePropertyChanged();
                if(value != null)
                {
                    value.Command.Execute(null);
                }
            }
        }
    }
}
