using LagoVista.IoT.DeviceAdmin.Models;
using System;
using Windows.UI.Xaml.Controls;

namespace LagoVista.UWP.UI.Controls.EntryCells
{
    public class TextEntryCell : CellBase
    {
        TextBox _txtBox;

        public TextEntryCell(CustomField _field) : base(_field)
        {
            _txtBox = new TextBox();
            _txtBox.TextChanged += _txtBox_TextChanged;
            Children.Add(_txtBox);
        }

        private void _txtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(Value != null)
            {
                Value.Value = _txtBox.Text;
            }
        }

        public override void UpdateSource()
        {
        
        }

        public override void UpdateTarget()
        {
            if (Value != null && Value.Value != null)
            {
                if (Value.Value == null)
                {
                    _txtBox.Text = String.Empty;
                    Value.Value = String.Empty;
                }
                else
                {
                    _txtBox.Text = Value.Value;
                }
            }
        }
    }
}
