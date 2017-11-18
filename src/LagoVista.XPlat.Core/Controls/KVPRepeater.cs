using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{
    public class KVPRepeater : StackLayout
    {
        private IEnumerable<KeyValuePair<string, object>> ItemSource
        {
            get { return (IEnumerable<KeyValuePair<string, object>>)base.GetValue(ItemSourceProperty); }
            set
            {
                base.SetValue(ItemSourceProperty, value);
                this.Children.Clear();
                var grid = new Grid();
                for (var idx = 0; idx < value.Count(); ++idx)
                {
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                }

                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(33, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(66, GridUnitType.Star) });

                var rowIdx = 0;
                foreach (var pair in value)
                {
                    var label = new Label();
                    label.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    label.TextColor = LabelColor;
                    label.FontAttributes = FontAttributes.Bold;

                    var valueLabel = new Label();
                    valueLabel.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    valueLabel.TextColor = ValueColor;
                    label.SetValue(Grid.ColumnProperty, 0);
                    valueLabel.SetValue(Grid.ColumnProperty, 1);

                    label.SetValue(Grid.RowProperty, rowIdx);
                    valueLabel.SetValue(Grid.RowProperty, rowIdx);

                    label.Text = pair.Key;
                    if (string.IsNullOrEmpty((string)pair.Value))
                    {
                        valueLabel.Text = "?";
                    }
                    else
                    {
                        valueLabel.Text = pair.Value.ToString();
                    }

                    grid.Children.Add(label);
                    grid.Children.Add(valueLabel);

                    ++rowIdx;
                }



                Children.Add(grid);
            }
        }

        public static BindableProperty LabelColorProperty = BindableProperty.Create(
                                                    propertyName: nameof(LabelColor),
                                                    returnType: typeof(Color),
                                                    declaringType: typeof(FormViewer),
                                                    defaultValue: Color.Black,
                                                    defaultBindingMode: BindingMode.Default,
                                                    propertyChanged: HandleLabelColorChanged);


        public Color LabelColor
        {
            get { return (Color)base.GetValue(LabelColorProperty);  }
            set { base.SetValue(LabelColorProperty, value);  }
        }

        private static void HandleLabelColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var kvpRepeater = (KVPRepeater)bindable;
            kvpRepeater.LabelColor = (Color)newValue;
        }

        public static BindableProperty ValueColorProperty = BindableProperty.Create(
                                            propertyName: nameof(ValueColor),
                                            returnType: typeof(Color),
                                            declaringType: typeof(FormViewer),
                                            defaultValue: Color.Black,
                                            defaultBindingMode: BindingMode.Default,
                                            propertyChanged: HandleValueColorChanged);


        public Color ValueColor
        {
            get { return (Color)base.GetValue(ValueColorProperty); }
            set { base.SetValue(ValueColorProperty, value); }
        }

        private static void HandleValueColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var kvpRepeater = (KVPRepeater)bindable;
            kvpRepeater.ValueColor = (Color)newValue;
        }

        public static BindableProperty ItemSourceProperty = BindableProperty.Create(
                                                    propertyName: nameof(ItemSource),
                                                    returnType: typeof(IEnumerable<KeyValuePair<string, object>>),
                                                    declaringType: typeof(FormViewer),
                                                    defaultValue: null,
                                                    defaultBindingMode: BindingMode.Default,
                                                    propertyChanged: HandleFormFieldsAssigned);

        private static void HandleFormFieldsAssigned(BindableObject bindable, object oldValue, object newValue)
        {
            var kvpRepeater = (KVPRepeater)bindable;
            kvpRepeater.ItemSource = newValue as IEnumerable<KeyValuePair<string, object>>;
        }
        
    }
}
