using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.FormControls
{
    public class FormFieldValidationMessage : Label
    {
        public FormFieldValidationMessage() : base()
        {
            TextColor = Color.Red;
            IsVisible = false;
        }

        public FormFieldValidationMessage(string value) : base()
        {
            TextColor = Color.Red;
            IsVisible = false;
            Text = value;
        }
    }
}