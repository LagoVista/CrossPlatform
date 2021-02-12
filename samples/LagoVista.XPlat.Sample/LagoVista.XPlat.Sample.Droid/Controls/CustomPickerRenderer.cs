using Android.Content;
using Android.Graphics.Drawables;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(LagoVista.XPlat.Core.Picker), typeof(LagoVista.XPlat.Droid.Controls.CustomPickerRenderer))]
namespace LagoVista.XPlat.Droid.Controls
{
    public class CustomPickerRenderer : PickerRenderer
    {
        public CustomPickerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                var pix = Resources.DisplayMetrics.Density;
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