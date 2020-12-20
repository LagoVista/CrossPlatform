using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(LagoVista.XPlat.Core.HyperLinkLabel), typeof(LagoVista.XPlat.Droid.Controls.HyperLinkLabelRenderer))]
namespace LagoVista.XPlat.Droid.Controls
{
    public class HyperLinkLabelRenderer : LabelRenderer
    {
        public HyperLinkLabelRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.PaintFlags |= Android.Graphics.PaintFlags.UnderlineText;
            }
        }
    }
}