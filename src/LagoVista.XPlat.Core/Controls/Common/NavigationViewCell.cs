using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
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

        private TapGestureRecognizer _tapGestureRecognizer;

        public NavigationViewCell()
        {
            _icon = new Icon()
            {
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                FontSize = (double)base.View.Resources["ListItemFontSize"],
                TextColor = (Color)View.Resources["ListItemLabelColor"],
            };
            _icon.SetValue(Grid.ColumnProperty, 0);
            _icon.SetValue(Grid.RowSpanProperty, 2);

            _text = new Label()
            {
                FontSize = (double)base.View.Resources["ListItemFontSize"],
                TextColor = (Color)View.Resources["ListItemIconColor"],
                FontFamily = (string)View.Resources["ListItemFont"],
            };
            _text.SetValue(Grid.ColumnProperty, 1);

            _detail = new Label()
            {
                FontSize = (double)base.View.Resources["LiteItemDetailSize"],
                TextColor = (Color)View.Resources["ListItemDetailColor"],
                FontFamily = (string)View.Resources["ListItemDetailFont"],
            };
            _detail.SetValue(Grid.ColumnProperty, 1);
            _detail.SetValue(Grid.RowProperty, 1);

            var cheveron = new Icon()
            {
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                FontSize = (double)base.View.Resources["ListItemFontSize"],
                TextColor = (Color)View.Resources["NavIconColor"],
                IconKey = "fa-chevron-right"
            };
            cheveron.SetValue(Grid.ColumnProperty, 2);
            cheveron.SetValue(Grid.RowSpanProperty, 2);

            _layout = new Grid();
            _layout.Margin = new Thickness(10);
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Absolute) });
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Absolute) });

            _layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            _layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            _layout.Children.Add(_icon);
            _layout.Children.Add(_text);
            _layout.Children.Add(_detail);
            _layout.Children.Add(cheveron);

            this.View = _layout;            
            _tapGestureRecognizer = new TapGestureRecognizer() { Command = Command };
            _layout.GestureRecognizers.Add(_tapGestureRecognizer);
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string),
            typeof(NavigationViewCell), string.Empty, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._text.Text = (string)newValue);

        public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Text), typeof(string),
         typeof(NavigationViewCell), string.Empty, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._detail.Text = (string)newValue);

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand),
            typeof(NavigationViewCell), null, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._tapGestureRecognizer.Command = (ICommand)newValue);

        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Text), typeof(string),
            typeof(NavigationViewCell), string.Empty, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._icon.IconKey = (string)newValue);

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        public string Detail
        {
            get { return (string)base.GetValue(DetailProperty); }
            set { base.SetValue(DetailProperty, value); }
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
