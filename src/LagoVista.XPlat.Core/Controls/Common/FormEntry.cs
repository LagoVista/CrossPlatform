using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.XPlat.Core.Services;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{
    /// <summary>
    /// Entry has a customer renderer that will leave out the underbar, this is useful with borders around text boxes
    /// </summary>
    public class Entry : Xamarin.Forms.Entry
    {
        public Entry()
        {
            this.Focused += Entry_Focused;

            FontFamily = ResourceSupport.GetString("EntryFont");
            FontSize = ResourceSupport.GetNumber("EntryFontSize");
            PlaceholderColor = ResourceSupport.GetColor("EntryPlaceholderColor");
            BackgroundColor = ResourceSupport.GetColor("EditControlBackground");
            TextColor = ResourceSupport.GetColor("EditControlText");

            if (Device.RuntimePlatform == Device.Android)
            {
                HeightRequest = 50;
            }
        }

        private void Entry_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
        }
    }

    /// <summary>
    /// Form Entry will draw an underline
    /// </summary>
    public class FormEntry : Xamarin.Forms.Entry
    {
        public FormEntry()
        {
            this.Focused += Entry_Focused;

            FontFamily = ResourceSupport.GetString("EntryFont");
            FontSize = ResourceSupport.GetNumber("EntryFontSize");

            BackgroundColor = ResourceSupport.GetColor("EditControlBackground");
            TextColor = ResourceSupport.GetColor("EditControlText");

            if (Device.RuntimePlatform == Device.Android)
            {
                HeightRequest = 40;
            }
        }

        private void Entry_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
        }
    }

    public class TextArea : Xamarin.Forms.Editor
    {
        public TextArea()
        {

        }
    }
}
