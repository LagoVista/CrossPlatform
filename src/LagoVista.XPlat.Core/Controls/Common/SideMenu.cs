using LagoVista.Client.Core.Resources;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.XPlat.Core.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.Common
{
    public class SideMenu : ScrollView
    {
        StackLayout _container;
        public event EventHandler<Client.Core.ViewModels.MenuItem> MenuItemTapped;
        Label _orgLabel;
        IAuthManager _autoManager;


        public SideMenu(IAuthManager authManager)
        {
            _container = new StackLayout();
            authManager.OrgChanged += AuthManager_OrgChanged;
            Content = _container;
            _autoManager = authManager;
            this.BackgroundColor = Color.FromRgb(0x3f, 0x3F, 0x3f);
            this.SetOnAppTheme<Color>(SideMenu.BackgroundColorProperty, ResourceSupport.GetColor("MenuBarBackgroundLight"), ResourceSupport.GetColor("MenuBarBackgroundDark"));
        }

        private void AuthManager_OrgChanged(object sender, LagoVista.Core.Models.EntityHeader e)
        {
            _orgLabel.Text = e.Text;
        }

        IEnumerable<LagoVista.Client.Core.ViewModels.MenuItem> _menuItems;
        public IEnumerable<LagoVista.Client.Core.ViewModels.MenuItem> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _container.Children.Clear();

                var lbl = new Label
                {
                    Text = ClientResources.CurrentOrganization_Label,
                    FontSize = ResourceSupport.GetNumber("MenuFontSize"),
                    Margin = new Thickness(44, 10, 0, 0)
                };

                _container.Children.Add(lbl);

                _orgLabel = new Label
                {
                    FontSize = ResourceSupport.GetNumber("HeaderFontSize"),
                };

                if (_autoManager.IsAuthenticated)
                {
                    this.IsVisible = true;
                    _orgLabel.Text = (_autoManager.User.CurrentOrganization != null) ? _autoManager.User.CurrentOrganization.Text : ClientResources.MainMenu_NoOrganization;
                }
                else
                {
                    this.IsVisible = false;
                }

                _orgLabel.Margin = new Thickness(44, 0, 0, 10);
                _container.Children.Add(_orgLabel);

                _menuItems = value;

                if (_menuItems != null)
                {
                    foreach (var menuItem in _menuItems)
                    {
                        menuItem.MenuItemTapped += MenuItem_MenuItemTapped;
                        _container.Children.Add(new SideMenuItem(menuItem));
                    }
                }

                if (Device.RuntimePlatform != Device.UWP)
                {
                    _orgLabel.FontFamily = ResourceSupport.GetString("HeaderFont");
                    lbl.FontFamily = ResourceSupport.GetString("MenuFont");
                    lbl.TextColor = ResourceSupport.GetColor("MenuBarTitle");
               
                }
            }
        }

        private void MenuItem_MenuItemTapped(object sender, Client.Core.ViewModels.MenuItem e)
        {
            MenuItemTapped?.Invoke(sender, e);
        }
    }
}
