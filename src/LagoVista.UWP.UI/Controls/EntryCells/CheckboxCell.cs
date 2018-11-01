using System;
using LagoVista.IoT.DeviceAdmin.Models;
using Windows.UI.Xaml.Controls;

namespace LagoVista.UWP.UI.Controls.EntryCells
{
    public class CheckboxCell : CellBase
    {
         ToggleSwitch _onOff;

        public CheckboxCell(CustomField _field) : base(_field)
        {
            _onOff = new ToggleSwitch();
            _onOff.Toggled += _onOff_Checked;
            Children.Add(_onOff);
        }

        private void _onOff_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Value.Value = _onOff.IsOn.ToString();
        }

        public override void UpdateSource()
        {
            
        }

        public override void UpdateTarget()
        {
            bool isCheckedDefault = false;

            if (!String.IsNullOrEmpty(Field.DefaultValue))
            {
                bool.TryParse(Value.Value, out bool isChecked);
             }

            if (Value != null && Value.Value != null)
            {
                if(bool.TryParse(Value.Value, out bool isChecked))
                {
                    _onOff.IsOn = isChecked;
                }
                else
                {
                    _onOff.IsOn = isCheckedDefault;
                }
            }
            else
            {
                _onOff.IsOn = isCheckedDefault;
            }
        }
    }
}
