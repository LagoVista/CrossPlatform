using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.XPlat.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.Common
{
    public class SideMenuItem : Grid
    {
        Label _menuText;
        Icon _icon;

        LagoVista.Client.Core.ViewModels.MenuItem _menuItem;

        public SideMenuItem(LagoVista.Client.Core.ViewModels.MenuItem menuItem)
        {
            HeightRequest = 32;

            _icon = new Icon();
            _icon.HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false);
            _icon.VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false);
            _icon.FontSize = ResourceSupport.GetNumber( "MenuFontSize");
            _icon.TextColor = ResourceSupport.GetColor("MenuIconColor");
            _icon.Margin = new Thickness(8, 4, 0, 0);
            _icon.IconKey = menuItem.FontIconKey;
            _menuItem = menuItem;

            _menuItem.Command.CanExecuteChanged += Command_CanExecuteChanged;

            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(36) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1,GridUnitType.Star) });

            _menuText = new Label();
            _menuText.VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false);
            _menuText.FontSize = ResourceSupport.GetNumber("MenuFontSize");
            _menuText.TextColor = ResourceSupport.GetColor("MenuBarForeground");
            _menuText.FontFamily = ResourceSupport.GetString("MenuFont");
            _menuText.SetValue(Grid.ColumnProperty, 1);
            _menuText.Text = menuItem.Name;

            Children.Add(_icon);
            Children.Add(_menuText);

            var tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.Tapped += TapRecognizer_Tapped;
            this.GestureRecognizers.Add(tapRecognizer);
        }

        private void TapRecognizer_Tapped(object sender, EventArgs e)
        {
            if(_menuItem.Command.CanExecute(_menuItem.CommandParameter))
            {
                _menuItem.RaiseMenuItemTapped();
                _menuItem.Command.Execute(_menuItem.CommandParameter);
            }
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform != Device.UWP)
            {
                if (_menuItem.Command.CanExecute(_menuItem.CommandParameter))
                {
                    _icon.TextColor = ResourceSupport.GetColor("MenuIconColor");
                    _menuText.TextColor = ResourceSupport.GetColor("MenuFontColor");
                }
                else
                {
                    BackgroundColor = ResourceSupport.GetColor("MenuBarDisableddDisabled");
                    _icon.TextColor = ResourceSupport.GetColor("MenuBarForegroundDisabled");
                    _menuText.TextColor = ResourceSupport.GetColor("MenuBarForegroundDisabled");
                }
            }
        }
     }
}
