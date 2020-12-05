using LagoVista.XPlat.UWP.CustomRenderers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(LagoVista.XPlat.Core.IconButton), typeof(IconButtonRendereer))]
namespace LagoVista.XPlat.UWP.CustomRenderers
{
    public class IconButtonRendereer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<global::Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control button = Control;
                var template = Windows.UI.Xaml.Application.Current.Resources["IconButtonStyle"] as Style;
                button.Style = template;
            }
        }
    }
}
