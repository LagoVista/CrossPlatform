using LagoVista.XPlat.Core.Resources;
using LagoVista.XPlat.UWP.CustomRenderers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(LagoVista.XPlat.Core.HyperLinkLabel), typeof(HyperLinkLabelRenderer))]
namespace LagoVista.XPlat.UWP.CustomRenderers
{
    public class HyperLinkLabelRenderer : LabelRenderer
    {

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Label> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.PointerEntered += Control_PointerEntered;
                Control.PointerExited += Control_PointerExited;
            }
        }

        private void Control_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var tb = sender as TextBlock;
            tb.Foreground = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources[nameof(IMobileStyle.LinkColor)]);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Control_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var tb = sender as TextBlock;
            tb.Foreground = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources[nameof(IMobileStyle.LinkActive)]);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        }
    }
}
