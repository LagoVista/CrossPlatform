using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using LagoVista.IoT.DeviceAdmin.Models;
using Windows.UI.Xaml;
using LagoVista.UWP.UI.Controls.EntryCells;
using LagoVista.IoT.DeviceManagement.Core.Models;

namespace LagoVista.UWP.UI.Controls
{
    public class DynamicFormList : StackPanel
    {
        private void RenderList(IEnumerable<CustomField> items)
        {
            Children.Clear();
            foreach (var formItem in items)
            {


                CellBase cell = null;

                switch (formItem.FieldType.Value)
                {
                    case ParameterTypes.DateTime:
                    case ParameterTypes.Decimal:
                    case ParameterTypes.GeoLocation:
                    case ParameterTypes.Integer:
                    case ParameterTypes.String:
                    case ParameterTypes.ValueWithUnit:
                        cell = new TextEntryCell(formItem);
                        break;
                    case ParameterTypes.State:
                        cell = new ListSelectCell(formItem);
                        break;
                    case ParameterTypes.TrueFalse:
                        cell = new CheckboxCell(formItem);
                        break;
                }

                Children.Add(cell);

                if (Values != null)
                {
                    var attrValue = Values.Where(val => val.Key == formItem.Key).FirstOrDefault();
                    if (attrValue != null)
                    {
                        cell.Value = attrValue;
                    }
                }
            }
        }

        public static readonly DependencyProperty FormItemsProperty = DependencyProperty.Register(nameof(FormItems), typeof(IEnumerable<CustomField>), typeof(DynamicFormList), new PropertyMetadata(null, new PropertyChangedCallback(FormItemsChanged)));

        public static void FormItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var ctl = d as DynamicFormList;
            ctl.FormItems = args.NewValue as IEnumerable<CustomField>;
        }

        public IEnumerable<CustomField> FormItems
        {
            get { return GetValue(FormItemsProperty) as IEnumerable<CustomField>; }
            set
            {
                SetValue(FormItemsProperty, value);
                if (value != null)
                {
                    RenderList(value);
                }
            }
        }


        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(nameof(Values), typeof(List<AttributeValue>), typeof(DynamicFormList), new PropertyMetadata(null, new PropertyChangedCallback(ValuePropertyChanged)));

        public static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var ctl = d as DynamicFormList;
            ctl.Values = args.NewValue as List<AttributeValue>;
        }

        public List<AttributeValue> Values
        {
            get { return GetValue(ValuesProperty) as List<AttributeValue>; }
            set
            {
                SetValue(ValuesProperty, value);

                var items = value as List<AttributeValue>;
                foreach (var child in Children)
                {
                    if (child is CellBase cell)
                    {
                        var attr = items.Where(itm => itm.Key == cell.Field.Key).FirstOrDefault();
                        if (attr == null)
                        {
                            attr = new AttributeValue()
                            {
                                Key = cell.Field.Key,
                                AttributeType = cell.Field.FieldType,
                                Value = cell.Field.DefaultValue,
                                Name = cell.Field.Name,
                                StateSet = cell.Field.StateSet,
                                UnitSet = cell.Field.UnitSet
                            };
                            Values.Add(attr);
                        }

                        cell.Value = attr;
                    }
                }
            }
        }
    }
}
