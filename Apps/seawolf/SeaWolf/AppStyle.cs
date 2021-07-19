using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.Drawing;

namespace SeaWolf
{
    public class AppStyle : IAppStyle
    {
        private readonly Color _black = Color.CreateColor(0, 0, 0);
        private readonly Color _white = Color.CreateColor(0xFF, 0xFF, 0xFF);
        private readonly Color _darkGray = Color.CreateColor(0x20, 0x20, 0x20);
        private readonly Color _medGray = Color.CreateColor(0x60, 0x60, 0x60);
        private readonly Color _lightGray = Color.CreateColor(0xA0, 0xA0, 0xA0);

        private const string DefaultFont = "Roboto";

        public Color TitleBarBackground => NamedColors.NuvIoTDark;

        public Color TitleBarText => NamedColors.NuvIoTWhite;

        public Color HighlightColor => NamedColors.NuvIoTContrast;


        public Color PageBackground => NamedColors.NuvIoTWhite;

        public Color PageText => _darkGray;

        public Color LabelText => _lightGray;

        public Color EditControlBackground => _white;

        public Color EditControlText => _black;

        public Color EditControlFrame => _black;

        public Color EditControlFrameFocus => _darkGray;

        public Color EditControlFrameInvalid => _black;


        public Color MenuBarBackground => NamedColors.NuvIoTMedium;

        public Color MenuBarForeground => NamedColors.NuvIoTLight;

        public Color MenuBarBackgroundActive => NamedColors.NuvIoTDark;

        public Color MenuBarForegroundActive => NamedColors.NuvIoTLight;


        public Color ButtonBackground => NamedColors.NuvIoTDark;

        public Color ButtonBorder => _black;

        public Color ButtonBackgroundActive => NamedColors.NuvIoTLight;

        public Color ButtonBorderActive => _darkGray;

        public Color ButtonForeground => NamedColors.NuvIoTWhite;

        public Color ButtonForegroundActive => NamedColors.NuvIoTWhite;

        public Color RowSeperatorColor => NamedColors.NuvIoTDark;

        public Color ListItemColor => NamedColors.NuvIoTDark;

        public Color TabForground => NamedColors.NuvIoTDark;
        public Color TabForgroundActive => NamedColors.NuvIoTLight;
        public Color TabBackgroundActive => NamedColors.NuvIoTDark;
        public Color TabBackground => NamedColors.NuvIoTLight;
        public Color TabBarBackground => NamedColors.NuvIoTMedium;


        public string HeaderFont => DefaultFont;

        public string ContentFont => DefaultFont;

        public string LabelFont => DefaultFont;

        public string EntryFont => DefaultFont;

        public string MenuFont => DefaultFont;

        public string ListItemFont => DefaultFont;

        public double HeaderFontSize => 24;

        public double LabelFontSize => 12;

        public double EntryFontSize => 12;

        public double ContentFontSize => 12;

        public double MenuFontSize => 18;

        public double ListItemFontSize => 28;

        public string TabBarFont => DefaultFont;

        public double TabBarIconFontSize => 32;

        public double TabBarFontSize => 18;

        public Color SectionHeaderColor => NamedColors.Black;

        public Color ListItemDetailColor => NamedColors.Black;
    }
}
