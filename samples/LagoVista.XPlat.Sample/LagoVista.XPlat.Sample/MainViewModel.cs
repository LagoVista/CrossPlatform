using System.Threading.Tasks;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Models.UIMetaData;
using System.Collections.Generic;

namespace LagoVista.XPlat.Sample
{
    public class MainViewModel : XPlatViewModel
    {

        public override Task InitAsync()
        {
            var fields = new Dictionary<string, FormField>();

            fields.Add("textEdit", FormField.Create("textEdit", new LagoVista.Core.Attributes.FormFieldAttribute(
                 FieldType:LagoVista.Core.Attributes.FieldTypes.Text
                )));
            

            FormAdapter = new EditFormAdapter(this, fields, this.ViewModelNavigation);

            return base.InitAsync();
        }

        public EditFormAdapter FormAdapter  { get;private set; }

    }

}
