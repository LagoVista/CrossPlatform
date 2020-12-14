using LagoVista.XPlat.UWP.CustomRenderers;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Picker), typeof(ComboBoxRenderer))]
namespace LagoVista.XPlat.UWP.CustomRenderers
{
    public class ComboBoxRenderer : Xamarin.Forms.Platform.UWP.PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control comboBox = Control;
             //   var template = Windows.UI.Xaml.Application.Current.Resources["LGVComboBox"] as Windows.UI.Xaml.Style;
               // comboBox.Style = template;
            }
        }
    }
}
