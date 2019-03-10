using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            FontFamily = "Roboto";
            this.BackgroundColor = AppStyle.EditControlBackground.ToXamFormsColor();

            if (Device.RuntimePlatform == Device.Android)
            {
                HeightRequest = 50;
            }
        }

        private void Entry_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
        }

        private IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }
    }

    /// <summary>
    /// Form Entry will draw an underline
    /// </summary>
    public class FormEntry : Xamarin.Forms.Entry
    {
        public FormEntry()
        {
            this.Focused += Entry_Focused;
            this.BackgroundColor = AppStyle.EditControlBackground.ToXamFormsColor();
            this.TextColor = AppStyle.EditControlText.ToXamFormsColor();

            if (Device.RuntimePlatform == Device.Android)
            {
                HeightRequest = 40;
            }
        }

        private void Entry_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
        }

        private IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }
    }

    public class TextArea : Xamarin.Forms.Editor
    {
        public TextArea()
        {

        }
    }
}
