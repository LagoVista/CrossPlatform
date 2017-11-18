using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Core
{
    public class Entry : Xamarin.Forms.Entry
    {
        public Entry()
        {
            this.Focused += Entry_Focused;
            this.BackgroundColor = AppStyle.EditControlBackground.ToXamFormsColor();
            this.TextColor = AppStyle.EditControlText.ToXamFormsColor();
            this.HeightRequest = 36;
        }

        private void Entry_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
        }

        private IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }
    }
}
