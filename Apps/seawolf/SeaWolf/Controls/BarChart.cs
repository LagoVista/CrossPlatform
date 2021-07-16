using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SeaWolf.Controls
{
    public class BarChart : Grid
    {
        Frame _lowRectangle;
        Frame _warningRectangle;
        Frame _okRectangle;

        public BarChart()
        {
            BackgroundColor = Color.Black;

            _lowRectangle = new Frame() { BackgroundColor = Color.Red, Margin = new Thickness(0, -6, 0, 0), CornerRadius=0, BorderColor = Color.Red };
            _warningRectangle = new Frame() { BackgroundColor = Color.Yellow, Margin = new Thickness(0, -6, 0, 0), CornerRadius = 0, BorderColor = Color.Yellow };
            _okRectangle = new Frame() { BackgroundColor = Color.Green, Margin = new Thickness(0), CornerRadius = 0, BorderColor = Color.Green };
         
            _okRectangle.SetValue(Grid.RowProperty, 0);
            _warningRectangle.SetValue(Grid.RowProperty, 1);
            _lowRectangle.SetValue(Grid.RowProperty, 2);

            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(11, GridUnitType.Star) });

            Children.Add(_lowRectangle);
            Children.Add(_warningRectangle);
            Children.Add(_okRectangle);
        }

        public static BindableProperty MinProperty = BindableProperty.Create(nameof(Min), typeof(double), typeof(BarChart), null, BindingMode.TwoWay, null,
           (obj, oldValue, newValue) =>
           {
               var ctl = obj as BarChart;
               ctl.Min = (float)newValue;
           });

        public static BindableProperty MaxProperty = BindableProperty.Create(nameof(Max), typeof(double), typeof(BarChart), null, BindingMode.TwoWay, null,
           (obj, oldValue, newValue) =>
           {
               var ctl = obj as BarChart;
               ctl.Max = (double)newValue;
           });

        public static BindableProperty LowThresholdProperty = BindableProperty.Create(nameof(LowThreshold), typeof(double), typeof(BarChart), null, BindingMode.TwoWay, null,
           (obj, oldValue, newValue) =>
           {
               var ctl = obj as BarChart;
               ctl.Min = (float)newValue;
           });

        public static BindableProperty OkThresholdProperty = BindableProperty.Create(nameof(OkThreshold), typeof(double), typeof(BarChart), null, BindingMode.TwoWay, null,
           (obj, oldValue, newValue) =>
           {
               var ctl = obj as BarChart;
               ctl.Max = (double)newValue;
           });

        public static BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(BarChart), null, BindingMode.TwoWay, null,
           (obj, oldValue, newValue) =>
           {
               var ctl = obj as BarChart;
               ctl.Max = (double)newValue;
           });


        public double Min
        {
            get => (double)GetValue(BarChart.MinProperty);
            set => SetValue(BarChart.MinProperty, value);
        }

        public double Max
        {
            get => (double)GetValue(BarChart.MaxProperty);
            set => SetValue(BarChart.MaxProperty, value);
        }

        public double OkThreshold
        {
            get => (double)GetValue(BarChart.OkThresholdProperty);
            set
            {
                SetValue(BarChart.OkThresholdProperty, value);
            }
        }

        public double LowThreshold
        {
            get => (double)GetValue(BarChart.LowThresholdProperty);
            set => SetValue(BarChart.LowThresholdProperty, value);
        }

        public double Value
        {
            get => (double)GetValue(BarChart.ValueProperty);
            set  
            {
                SetValue(BarChart.ValueProperty, value);
            }
        }

    }
}
