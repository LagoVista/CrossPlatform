using LagoVista.Client.Core.Icons;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using System;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{
    /// <summary>
    /// Defines the <see cref="IconButton" /> control.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.Button" />
    public class IconButton : Button
    {
        public IconButton()
        {
            BackgroundColor = Xamarin.Forms.Color.Transparent;
            TextColor = AppStyle.TitleBarText.ToXamFormsColor();
        }

        private IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }

        public static BindableProperty IconKeyProperty = BindableProperty.Create(nameof(IconKey), typeof(string), typeof(IconButton), default(string), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as IconButton).IconKey = (string)newValue);

        public string IconKey
        {
            get { return (string)GetValue(IconButton.IconKeyProperty); }
            set
            {
                SetValue(IconButton.IconKeyProperty, value);
                var icon = Iconize.FindIconForKey(value);
                if (icon == null)
                {
                    throw new Exception("Could not find icon for: " + value);
                }

                switch (Device.RuntimePlatform)
                {
                    case Device.UWP: FontFamily = $"{Iconize.FindModuleOf(icon).FontPath}#{Iconize.FindModuleOf(icon).FontName}"; break;
                    case Device.iOS: FontFamily = Iconize.FindModuleOf(icon).FontName; break;
                    case Device.Android: FontFamily = $"{Iconize.FindModuleOf(icon).FontPath}#{Iconize.FindModuleOf(icon).FontName}"; break;
                }

                Text = $"{icon.Character}";
            }
        }

        public object Tag { get; set; }
    }
}
