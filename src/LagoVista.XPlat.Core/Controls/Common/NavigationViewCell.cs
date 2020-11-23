using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using System.Windows.Input;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.Common
{
    public class NavigationViewCell : ViewCell
    {
        Label _text;
        Grid _layout;
        Icon _icon;

        private TapGestureRecognizer _tapGestureRecognizer;

        public NavigationViewCell()
        {
            _icon = new Icon()
            {
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                FontSize = AppStyle.ListItemFontSize,
                TextColor = AppStyle.HighlightColor.ToXamFormsColor(),
            };
            _icon.SetValue(Grid.ColumnProperty, 0);

            _text = new Label()
            {
                FontSize = AppStyle.ListItemFontSize,
                TextColor = AppStyle.ListItemColor.ToXamFormsColor(),
                FontFamily = AppStyle.ListItemFont,
            };
            _text.SetValue(Grid.ColumnProperty, 1);

            var cheveron = new Icon()
            {
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                FontSize = AppStyle.ListItemFontSize,
                TextColor = AppStyle.HighlightColor.ToXamFormsColor(),
                IconKey = "fa-chevron-right"
            };
            cheveron.SetValue(Grid.ColumnProperty, 2);

            _layout = new Grid();
            _layout.Margin = new Thickness(10);
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Absolute) });
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            _layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Absolute) });

            _layout.Children.Add(_icon);
            _layout.Children.Add(_text);
            _layout.Children.Add(cheveron);


            this.View = _layout;            
            _tapGestureRecognizer = new TapGestureRecognizer() { Command = Command };
         //   this.View.GestureRecognizers.Add(_tapGestureRecognizer);
            _layout.GestureRecognizers.Add(_tapGestureRecognizer);
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string),
            typeof(NavigationViewCell), string.Empty, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._text.Text = (string)newValue);

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand),
            typeof(NavigationViewCell), null, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._tapGestureRecognizer.Command = (ICommand)newValue);

        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Text), typeof(string),
            typeof(NavigationViewCell), string.Empty, BindingMode.OneWay, null, (view, oldValue, newValue) => (view as NavigationViewCell)._icon.IconKey = (string)newValue);

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
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

        private IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }
    }
}
