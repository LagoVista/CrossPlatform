using LagoVista.Core.Models.UIMetaData;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.FormControls
{
    public class CheckBoxRow : FormControl
    {
        FormFieldHeader _header;
        Switch _switch;

        public CheckBoxRow(FormViewer formViewer, FormField field) : base(formViewer, field)
        {
            _header = new FormFieldHeader(field.Label);

            _switch = new Switch();
            _switch.Toggled += _switch_Toggled;

            bool _isToggled = false;
            if (bool.TryParse(field.Value, out _isToggled))
            {
                _switch.IsToggled = _isToggled;
            }

            Children.Add(_header);
            Children.Add(_switch);
            Margin = RowMargin;
        }

        private void _switch_Toggled(object sender, ToggledEventArgs e)
        {
            Field.Value = e.Value ? "true" : "false";
            IsDirty = OriginalValue != Field.Value;
        }

        public override bool Validate()
        {
            return true;
        }
    }
}
