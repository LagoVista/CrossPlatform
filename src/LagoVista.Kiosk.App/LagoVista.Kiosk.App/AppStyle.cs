﻿using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.Kiosk.App
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

        private const string DefaultFont = "Roboto";

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

        public Color MenuBarBackground => _medGray;

        public Color MenuBarForeground => NamedColors.NuvIoTContrast;

        public Color MenuBarBackgroundActive => _darkGray;

        public Color MenuBarForegroundActive => NamedColors.NuvIoTContrast;

        public Color ButtonBackground => NamedColors.NuvIoTDark;

        public Color ButtonBorder => _black;

        public Color ButtonBackgroundActive => NamedColors.NuvIoTLight;

        public Color ButtonBorderActive => _darkGray;

        public Color ButtonForeground => NamedColors.NuvIoTWhite;

        public Color ButtonForegroundActive => NamedColors.NuvIoTWhite;

        public Color HighlightColor => NamedColors.NuvIoTContrast;

        public Color RowSeperatorColor => NamedColors.NuvIoTDark;

        public Color ListItemColor => NamedColors.NuvIoTDark;

        public string HeaderFont => DefaultFont;

        public string ContentFont => DefaultFont;

        public string LabelFont => DefaultFont;

        public string EntryFont => DefaultFont;

        public string MenuFont => "Verdana";

        public string ListItemFont => DefaultFont;

        public double HeaderFontSize => 24;

        public double LabelFontSize => 12;

        public double EntryFontSize => 12;

        public double ContentFontSize => 12;

        public double MenuFontSize => 18;

        public double ListItemFontSize => 28;
    }
}
