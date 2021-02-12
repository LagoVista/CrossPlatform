using Android.Content;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Android.Graphics.Drawables;
using System;

[assembly: ExportRenderer(typeof(LagoVista.XPlat.Core.Entry), typeof(LagoVista.XPlat.Droid.Controls.CustomEntryRenderer))]
namespace LagoVista.XPlat.Droid.Controls
{

    public class CustomEntryRenderer : EntryRenderer
    {
        public CustomEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                var pix = Resources.DisplayMetrics.Density;

                Control.SetBackgroundColor(global::Android.Graphics.Color.White);
                var customBG = new GradientDrawable();
                customBG.SetCornerRadius(3);
                int borderWidth = 2;
                customBG.SetStroke(borderWidth, Android.Graphics.Color.Gray);
                Control.SetBackground(customBG);
                Control.SetPadding(Convert.ToInt32(pix * 10), 0, 0, 0);
                Control.SetHeight(Convert.ToInt32(32 * pix));
            }
        }
    }
}