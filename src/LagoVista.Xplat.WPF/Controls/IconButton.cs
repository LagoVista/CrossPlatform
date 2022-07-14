using FontAwesome5;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LagoVista.XPlat.WPF
{
    public class IconButton : Button
    {
        FontAwesome5.SvgAwesome _icon;

        public IconButton()
        {
            _icon = new FontAwesome5.SvgAwesome();
            _icon.Icon = EFontAwesomeIcon.Solid_Check;
            Content = _icon;
            BorderBrush = Brushes.Transparent;
            Background = Brushes.Transparent;
           
        }

        public static readonly DependencyProperty IconNameProperty = DependencyProperty.Register("IconName", typeof(EFontAwesomeIcon), typeof(IconButton), new PropertyMetadata(EFontAwesomeIcon.Solid_Check,
            new PropertyChangedCallback((obj, args) =>
        {
            var icon = (IconButton)obj;
            icon._icon.Icon = (EFontAwesomeIcon)args.NewValue;

        })));

        public EFontAwesomeIcon IconName
        {
            get => (EFontAwesomeIcon)GetValue(IconNameProperty);
            set
            {
                SetValue(IconNameProperty, value);
                _icon.Icon = value;
            }
        }

        public static new readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(IconButton), new PropertyMetadata(Brushes.Black,
            new PropertyChangedCallback((obj, args) =>
            {
                var icon = (IconButton)obj;
                icon._icon.Foreground = (Brush)args.NewValue;

            })));

        public new Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set
            {
                SetValue(ForegroundProperty, value);
                _icon.Foreground = value;
            }
        }

    }
}
