using Android.Content;
using Android.Graphics.Drawables;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(LagoVista.XPlat.Core.TextArea), typeof(LagoVista.XPlat.Droid.Controls.CustomTextAreaRenderer))]
namespace LagoVista.XPlat.Droid.Controls
{
    public class CustomTextAreaRenderer : EditorRenderer
    {
        public CustomTextAreaRenderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
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
            }
        }
    }
}