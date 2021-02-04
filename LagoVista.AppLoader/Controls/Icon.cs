using LagoVista.Client.Core.Icons;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LagoVista.AppLoader.Controls
{
    public class Icon : TextBlock
    {
        public static readonly DependencyProperty IconKeyProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(string),
            typeof(Icon), new PropertyMetadata(new PropertyChangedCallback(OnUriChanged)));

        public static Dictionary<string, FontFamily> _fonts = new Dictionary<string, FontFamily>();

        private static void OnUriChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var ctl = (Icon)obj;
                var icon = Iconize.FindIconForKey(e.NewValue.ToString());
                if (icon == null)
                {
                    throw new Exception("Could not find icon for: " + e.NewValue.ToString());
                }

                var module = Iconize.FindModuleOf(icon);

                if(_fonts.ContainsKey(module.FontPath))
                {
                    ctl.FontFamily = _fonts[module.FontPath];
                }
                else
                {
                    var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), module.FontPath);
                    _fonts.Add(module.FontPath, fontFamily);
                    ctl.FontFamily = fontFamily;
                }

                ctl.Text = $"{icon.Character}";
            }
        }

        public string IconKey
        {
            set { SetValue(IconKeyProperty, value); }
            get { return GetValue(IconKeyProperty) as String; }
        }
    }
}
