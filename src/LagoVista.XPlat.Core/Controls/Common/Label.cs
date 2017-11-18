using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Core
{
    public class Label : Xamarin.Forms.Label
    {
        public Label()
        {
            this.TextColor = AppStyle.LabelText.ToXamFormsColor();
        }

        private IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }
    }
}
