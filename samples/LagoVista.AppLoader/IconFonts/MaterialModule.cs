﻿using LagoVista.Client.Core.Icons;

namespace LagoVista.Core.WPF.IconFonts
{
    /// <summary>
    /// Defines the <see cref="MaterialModule" /> icon module.
    /// </summary>
    /// <seealso cref="Plugin.Iconize.IconModule" />
    public sealed class MaterialModule : IconModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialModule" /> class.
        /// </summary>
        public MaterialModule()
            : base("Material Icons", "MaterialIcons-Regular", "./Assets/Fonts/#Material Icons", MaterialCollection.Icons)
        {
            // Intentionally left blank
        }
    }
}