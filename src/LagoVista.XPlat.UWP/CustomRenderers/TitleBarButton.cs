using Xamarin.Forms.Platform.UWP;
using LagoVista.XPlat.UWP.CustomRenderers;
using LagoVista.XPlat.Core;

[assembly: ExportRenderer(typeof(IconButton), typeof(TitleBarButtonRenderer))]
namespace LagoVista.XPlat.UWP.CustomRenderers
{
    public class TitleBarButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<global::Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);
        }
    }
}
