using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Frame), typeof(SeaWolf.iOS.Renderers.FrameFixRenderer))]
namespace SeaWolf.iOS.Renderers
{
    public class FrameFixRenderer : FrameRenderer
    {

        Frame _control;

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if(e.OldElement != null && _control != null)
            {
                _control.PropertyChanged -= _control_PropertyChanged;
            }

            if (NativeView != null && Element != null)
            {
                _control = e.NewElement as Frame;
                _control.PropertyChanged += _control_PropertyChanged;
                UpdateCorners();
            }
        }
        protected void UpdateCorners()
        {
            NativeView.Layer.AllowsEdgeAntialiasing = true;
            NativeView.Layer.MasksToBounds = _control.IsClippedToBounds;
            NativeView.Layer.CornerRadius = _control.CornerRadius;
        }

        private void _control_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (NativeView == null || _control == null)
                return;

            if (e.PropertyName == "IsClippedToBounds" || e.PropertyName == "CornerRadius")
            {
                UpdateCorners();
            }
        }
    }
}