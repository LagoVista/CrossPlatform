using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using Windows.UI.Xaml.Controls;

namespace LagoVista.UWP.UI.Controls.EntryCells
{
    public abstract class CellBase : StackPanel
    {
        TextBlock _caption;
        CustomField _field;

        public CellBase(CustomField field)
        {
            _field = field;
            _caption = new TextBlock();
            _caption.Text = _field.Label;
            Children.Add(_caption);
        }

        AttributeValue _value;

        public AttributeValue Value
        {
            get { return _value; }
            set
            {
                _value = value;
                UpdateTarget();
            }
        }

        public abstract void UpdateTarget();

        public abstract void UpdateSource();

        public CustomField Field { get { return _field; } }
    }
}
