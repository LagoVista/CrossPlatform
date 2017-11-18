using LagoVista.Client.Core.Resources;
using LagoVista.Core.Interfaces;
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

                var lbl = new Label();
                lbl.Text = ClientResources.CurrentOrganization_Label;
                lbl.TextColor = Color.LightGray;
                lbl.FontSize = 18;
                lbl.Margin = new Thickness(44, 10, 0, 0);
                _container.Children.Add(lbl);

                
                var org = new Label();
                _orgLabel = new Label();
                _orgLabel.FontSize = 22;
                _orgLabel.TextColor = Color.White;
                _orgLabel.Text = (_autoManager.User.CurrentOrganization != null) ? _autoManager.User.CurrentOrganization.Text : ClientResources.MainMenu_NoOrganization;
                _orgLabel.Margin = new Thickness(44, 0, 0, 10);
                _container.Children.Add(_orgLabel);

                _menuItems = value;
                
                if (_menuItems != null)
                {
                    foreach (var menuItem in _menuItems)
                    {
                        menuItem.MenuItemTapped += MenuItem_MenuItemTapped1;
                        _container.Children.Add(new SideMenuItem(menuItem));
                    }
                }
            }
        }

        private void MenuItem_MenuItemTapped1(object sender, Client.Core.ViewModels.MenuItem e)
        {
            MenuItemTapped?.Invoke(sender, e);
        }
    }
}
