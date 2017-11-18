using LagoVista.Core.Models.UIMetaData;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.FormControls
{
    public class TextAreaRow : FormControl
    {
        FormFieldHeader _header;
        Editor _editor;

        public TextAreaRow(FormViewer formViewer, FormField field) : base(formViewer, field)
        {
            _header = new FormFieldHeader(field.Label);

            _editor = new Editor()
            {
                Text = field.Value,
                HeightRequest = 120
            };
            _editor.TextChanged += _editor_TextChanged;

            Children.Add(_header);
            Children.Add(_editor);
            Margin = RowMargin;
        }

        private void _editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            Field.Value = e.NewTextValue;
            Field.Value = e.NewTextValue;
            IsDirty = OriginalValue != Field.Value;
        }

        public override bool Validate()
        {
            return true;
        }
    }
}