using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SeaWolf.Extensions
{
    [ContentProperty("Text")]
    public class TranslationExtension : IMarkupExtension
    {

        // Look at: poeditor.com 

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return null;

            return SeawolfResources.ResourceManager.GetString(Text, CultureInfo.CurrentCulture);
        }
    }
}
