using LagoVista.Client.Core.Icons;

namespace LagoVista.XPlat.Droid.IconFonts
{
    /// <summary>
    /// Defines the <see cref="MeteoconsModule" /> icon module.
    /// </summary>
    /// <seealso cref="Plugin.Iconize.IconModule" />
    public sealed class MeteoconsModule : IconModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeteoconsModule" /> class.
        /// </summary>
        public MeteoconsModule()
            : base("Meteocons", "Meteocons", "Resources/Fonts/iconize-meteocons.ttf", MeteoconsCollection.Icons)
        {
            // Intentionally left blank
        }
    }
}