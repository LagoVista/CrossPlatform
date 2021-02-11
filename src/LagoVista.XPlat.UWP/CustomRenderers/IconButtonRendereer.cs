using LagoVista.XPlat.UWP.CustomRenderers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(LagoVista.XPlat.Core.IconButton), typeof(IconButtonRenderer), Priority =1)]
namespace LagoVista.XPlat.UWP.CustomRenderers
{
    public class IconButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<global::Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                if (Windows.UI.Xaml.Application.Current.Resources.Keys.Contains("IconButtonStyle"))
                {
                    Control button = Control;
                    var template = Windows.UI.Xaml.Application.Current.Resources["IconButtonStyle"] as Style;
                    button.Style = template;
                }
            }
        }
    }
}
