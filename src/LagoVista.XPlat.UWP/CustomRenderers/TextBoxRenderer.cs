using LagoVista.XPlat.UWP.CustomRenderers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(LagoVista.XPlat.Core.Entry), typeof(StandardTextBoxRenderer))]

namespace LagoVista.XPlat.UWP.CustomRenderers
{
    public class StandardTextBoxRenderer : Xamarin.Forms.Platform.UWP.EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<global::Xamarin.Forms.Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
               Control editControl = Control;
         //      Application.Current.Resources.TryGetValue("LGVDefaultTextBoxStyle", out var style);
           //    editControl.Style = style as Style;
            }
        }
    }
}
