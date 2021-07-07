using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Core.Commanding;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.Client.Core.ViewModels.Other;
using System.Threading.Tasks;

namespace LagoVista.PlatformManager.Core.ViewModels
{
    public class MainViewModel : ListViewModelBase<IoT.Deployment.Admin.Models.DeploymentInstanceSummary>
    {
        public MainViewModel()
        {
            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<UserOrgsViewModel>(this)),
                    Name = ClientResources.MainMenu_SwitchOrgs,
                    FontIconKey = "fa-users"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<ChangePasswordViewModel>(this)),
                    Name = ClientResources.MainMenu_ChangePassword,
                    FontIconKey = "fa-key"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<InviteUserViewModel>(this)),
                    Name = ClientResources.MainMenu_InviteUser,
                    FontIconKey = "fa-user"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<AboutViewModel>(this)),
                    Name = "About",
                    FontIconKey = "fa-info"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => Logout()),
                    Name = ClientResources.Common_Logout,
                    FontIconKey = "fa-sign-out"
                }
            };

            ShowIoTAppStudioCommand = new RelayCommand(() => NetworkService.OpenURI(new Uri("https://www.IoTAppStudio.com")));
        }

        public async override Task InitAsync()
        {
            await  base.InitAsync();
            if(ListItems != null)
            {
                HasPlatforms = ListItems.Any();
                EmptyPlatforms = !HasPlatforms;
            }
        }

        private bool _hasPlatforms;
        public bool HasPlatforms
        {
            get { return _hasPlatforms; }
            set { Set(ref _hasPlatforms, value); }
        }

        private bool _emptyPlatforms;
        public bool EmptyPlatforms
        {
            get { return _emptyPlatforms; }
            set { Set(ref _emptyPlatforms, value); }
        }

        public RelayCommand ShowIoTAppStudioCommand { get; private set; }

        protected override async void ItemSelected(DeploymentInstanceSummary model)
        {
            await NavigateAndViewAsync<InstanceViewModel>(model.Id);
        }

        protected override string GetListURI()
        {
            return $"/api/deployment/instances";
        }
    }
}
