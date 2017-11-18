using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LagoVista.Core.UWP.Services
{
    public class BindingHelper : IBindingHelper
    {
        public void RefreshBindings()
        {
            var element = FocusManager.GetFocusedElement();
            var textBox = element as TextBox;
            if (textBox != null)
                textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
