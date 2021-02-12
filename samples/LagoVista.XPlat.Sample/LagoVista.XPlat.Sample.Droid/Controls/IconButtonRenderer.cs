using System;
using Android.Content;
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
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                var pix = Resources.DisplayMetrics.Density;
                Control.SetPadding(0, 0, 0, 0);
            }
        }
    }
}