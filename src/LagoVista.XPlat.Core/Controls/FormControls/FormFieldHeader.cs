using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.FormControls
{
    public class FormFieldHeader : Label
    {
        public FormFieldHeader() : base()
        {
            TextColor = Color.FromRgb(0x5B, 0x5B, 0x5B);
        }

        public FormFieldHeader(string value) : base()
        {
            TextColor = Color.FromRgb(0x5B, 0x5B, 0x5B);
            Text = value;
        }
    }
}