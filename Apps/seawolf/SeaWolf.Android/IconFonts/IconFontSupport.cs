using LagoVista.Client.Core.Icons;

namespace LagoVista.XPlat.Droid.IconFonts
{
    public static class IconFontSupport
    {
        public static void RegisterFonts()
        {
            Iconize.With(new EntypoPlusModule())
                .With(new FontAwesomeModule())
                .With(new IoniconsModule())
                .With(new MaterialModule())
                .With(new MeteoconsModule())
                .With(new SimpleLineIconsModule())
                .With(new TypiconsModule())
                .With(new WeatherIconsModule());
        }
    }
}