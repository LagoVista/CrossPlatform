using Android.Content;

using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;

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
                Control.SetBackgroundColor(global::Android.Graphics.Color.White);
            }
        }
    }
}