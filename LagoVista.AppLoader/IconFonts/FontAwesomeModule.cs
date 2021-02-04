
using LagoVista.Client.Core.Icons;

namespace LagoVista.Core.WPF.IconFonts
{
    /// <summary>
    /// Defines the <see cref="FontAwesomeModule" /> icon module.
    /// </summary>
    /// <seealso cref="Plugin.Iconize.IconModule" />
    public sealed class FontAwesomeModule : IconModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FontAwesomeModule" /> class.
        /// </summary>
        public FontAwesomeModule()
            : base("FontAwesome", "FontAwesome", "./Assets/Fonts/#FontAwesome", FontAwesomeCollection.Icons)
        {
            // Intentionally left blank
        }
    }
}