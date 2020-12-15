﻿using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.XPlat.Core.Services;
using System.Windows.Input;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.Common
{
    public class NavigationViewCell : ViewCell
    {
        Label _text;
        Label _detail;
        Grid _layout;
        Icon _icon;
        Icon _chevron;

        private TapGestureRecognizer _tapGestureRecognizer;

        public NavigationViewCell()
        {
            _layout = new Grid();
            _layout.HeightRequest = 72;
     
            _icon = new Icon()
            {
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, true),
                FontSize = ResourceSupport.GetNumber("ListItemFontSize"),
                TextColor = ResourceSupport.GetColor("ListItemIconColor"),
            };
            
            _icon.SetValue(Grid.ColumnProperty, 0);
            _icon.SetValue(Grid.RowSpanProperty, 2);

            _text = new Label()
            {
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, true),
                FontSize = ResourceSupport.GetNumber("ListItemFontSize"),
                TextColor = ResourceSupport.GetColor("ListItemLabelColor"),
                FontFamily = ResourceSupport.GetString("ListItemFont"),
            };
            _text.SetValue(Grid.ColumnProperty, 1);
            _text.SetValue(Grid.RowSpanProperty, 2);

            _detail = new Label()
            {
                FontSize = ResourceSupport.GetNumber("LiteItemDetailSize"),
                TextColor = ResourceSupport.GetColor("ListItemDetailColor"),
                FontFamily = ResourceSupport.GetString("ListItemDetailFont"),
                IsVisible = false,
            };
            _detail.SetValue(Grid.ColumnProperty, 1);
            _detail.SetValue(Grid.RowProperty, 1);

            _chevron = new Icon()
            {
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, true),
                FontSize = ResourceSupport.GetNumber("ListItemFontSize"),
                TextColor = ResourceSupport.GetColor("NavIconColor"),
                IconKey = "fa-chevron-right",
            };
            _chevron.SetValue(Grid.ColumnProperty, 2);
            _chevron.SetValue(Grid.RowSpanProperty, 2);

            _layout.Margin = new Thickness(10);
            _layout.BackgroundColor = ResourceSupport.GetColor("ListItemBackgroundColor");
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Absolute) });
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Absolute) });

            _layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            _layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            _layout.Children.Add(_icon);
            _layout.Children.Add(_text);
            _layout.Children.Add(_detail);
            _layout.Children.Add(_chevron);

            this.View = _layout;
            _tapGestureRecognizer = new TapGestureRecognizer() { Command = Command };
            _layout.GestureRecognizers.Add(_tapGestureRecognizer);
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string),
            typeof(NavigationViewCell), string.Empty, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell).Text = (string)newValue);

        public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Text), typeof(string),
         typeof(NavigationViewCell), string.Empty, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell).Detail = (string)newValue);

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand),
            typeof(NavigationViewCell), null, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._tapGestureRecognizer.Command = (ICommand)newValue);

        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Text), typeof(string),
            typeof(NavigationViewCell), string.Empty, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._icon.IconKey = (string)newValue);

        public static readonly BindableProperty HasChevronProperty = BindableProperty.Create(nameof(HasChevron), typeof(bool),
         typeof(NavigationViewCell), true, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._chevron.IsVisible = (bool)newValue);

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set
            {
                _text.Text = value;
                base.SetValue(TextProperty, value);
            }
        }

        public string Detail
        {
            get { return (string)base.GetValue(DetailProperty); }
            set
            {
                _text.Margin = new Thickness(0, 10, 0, 0);
                _detail.Margin = new Thickness(0, 0, 0, 10);
                _detail.Text = value;
                _text.SetValue(Grid.RowSpanProperty, 1);
                _text.HeightRequest = 48;
                _detail.IsVisible = true;
                _layout.RowDefinitions[0] = new RowDefinition() { Height = new GridLength(3, GridUnitType.Star) };
                _layout.RowDefinitions[1] = new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) };
                base.SetValue(DetailProperty, value);
            }
        }

        public bool HasChevron
        {
            get { return (bool)base.GetValue(HasChevronProperty); }
            set { base.SetValue(HasChevronProperty, value);  }
        }

        public string Icon
        {
            get { return (string)base.GetValue(IconProperty); }
            set { base.SetValue(IconProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)base.GetValue(CommandProperty); }
            set { base.SetValue(CommandProperty, value); }
        }
    }
}
