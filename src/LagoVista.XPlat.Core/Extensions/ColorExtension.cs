using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista
{
    public static class ColorExtension
    {
        public static Xamarin.Forms.Color ToXamFormsColor(this LagoVista.Core.Models.Drawing.Color color)
        {
            return Xamarin.Forms.Color.FromRgba(color.R, color.G, color.B, color.A);
        }
    }
}
