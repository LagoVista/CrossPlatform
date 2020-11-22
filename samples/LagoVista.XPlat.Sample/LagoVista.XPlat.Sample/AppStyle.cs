using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample
{
    public class AppStyle : IAppStyle
    {
        private readonly Color _black = Color.CreateColor(0, 0, 0);
        private readonly Color _white = Color.CreateColor(0xFF, 0xFF, 0xFF);
        private readonly Color _darkGray = Color.CreateColor(0x20, 0x20, 0x20);
        private readonly Color _medGray = Color.CreateColor(0x60, 0x60, 0x60);
        private readonly Color _lightGray = Color.CreateColor(0xA0, 0xA0, 0xA0);
        private readonly Color _red = Color.CreateColor(0xFF, 0x0, 0x0);
        private readonly Color _blue = Color.CreateColor(0x0, 0x0, 0xFF);


        public Color TitleBarBackground => NamedColors.NuvIoTDark;

        public Color TitleBarText => NamedColors.NuvIoTWhite;

        public Color PageBackground => NamedColors.NuvIoTWhite;

        public Color PageText => _darkGray;

        public Color LabelText => _lightGray;

        public Color EditControlBackground => _white;

        public Color EditControlText => _black;

        public Color EditControlFrame => _black;

        public Color EditControlFrameFocus => _darkGray;

        public Color EditControlFrameInvalid => _black;

        public Color MenuBarBackground => _darkGray;

        public Color MenuBarForeground => _white;

        public Color MenuBarBackgroundActive => _darkGray;

        public Color MenuBarForegroundActive => _lightGray;

        public Color ButtonBackground => NamedColors.NuvIoTMedium;

        public Color ButtonBorder => _black;

        public Color ButtonBackgroundActive => NamedColors.NuvIoTLight;

        public Color ButtonBorderActive => _darkGray;

        public Color ButtonForeground => NamedColors.NuvIoTWhite;

        public Color ButtonForegroundActive => NamedColors.NuvIoTWhite;

        public Color HighlightColor => NamedColors.NuvIoTContrast;

        public Color RowSeperatorColor => _blue;

        public void Register(Xamarin.Forms.Application app)
        {
            var properties = GetType().GetProperties();
            foreach(var prop in properties)
            {
                app.Resources.Add(prop.Name, (prop.GetValue(this) as Color).ToXamFormsColor());
            }
        }
    }
}
