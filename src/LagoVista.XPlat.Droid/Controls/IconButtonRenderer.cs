using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using LagoVista.XPlat.Core;

[assembly: ExportRenderer(typeof(IconButton), typeof(LagoVista.XPlat.Droid.Controls.IconButtonRenderer))]
namespace LagoVista.XPlat.Droid.Controls
{
    
    public class IconButtonRenderer : ButtonRenderer
    {
        public IconButtonRenderer(Context context) : base(context)
        {
            Background = null;
        }
    }
}