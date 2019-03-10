using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.FormControls
{
    public class FormFieldHeader : Label
    {
        public FormFieldHeader() : base()
        {
            FontSize = 18;
            Margin = new Thickness(8, 5, 0, -5);
            FontAttributes = Xamarin.Forms.FontAttributes.Bold;
        }

        public FormFieldHeader(string value) : this()
        {
            Text = value;
        }
    }
}