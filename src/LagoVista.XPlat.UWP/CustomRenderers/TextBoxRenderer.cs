using LagoVista.XPlat.UWP.CustomRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Control button = Control;

               // Windows.UI.Xaml.Application.Current.Resources.TryGetValue("LGVDefaultTextBoxStyle", out var style);
               // button.Style = style as Style;
            }
        }
    }
}
