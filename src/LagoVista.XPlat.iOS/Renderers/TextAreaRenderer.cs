using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using LagoVista.XPlat.Core;
using LagoVista.XPlat.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TextArea), typeof(TextAreaRenderer))]

namespace LagoVista.XPlat.iOS.Renderers
{
    public class TextAreaRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Layer.BorderColor = UIColor.FromRGB(0xd4, 0xd4, 0xd4).CGColor;
                Control.Layer.BorderWidth = 0.75f;
                Control.Layer.CornerRadius = 8;
            }
        }
    }
}