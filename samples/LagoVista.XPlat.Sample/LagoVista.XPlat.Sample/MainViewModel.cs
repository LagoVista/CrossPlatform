using System.Threading.Tasks;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Models.UIMetaData;
using System.Collections.Generic;
using LagoVista.Core.Models;
using LagoVista.Core.Commanding;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.Other;

namespace LagoVista.XPlat.Sample
{
    public class MainViewModel : XPlatViewModel
    {

        public MainViewModel()
        {
            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<ServicesViewModel>(this)),
                    Name = "Services",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SecureStorageViewModel>(this)),
                    Name = "Secure Storage",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<LoginViewModel>(this)),
                    Name = "Login Page",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<AboutViewModel>(this)),
                    Name = "About",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<ControlSampleViewModel>(this)),
                    Name = "Control Sample",
                    FontIconKey = "fa-gear"
                },
            };
        }

        public override Task InitAsync()
        {
            var model1 = new Model1();
            model1.Model2Litems = new List<Model2>();
            var response = DetailResponse<Model1>.Create(model1);
            

            var frmEditPasswordLink = FormField.Create("EditPassword",
            new LagoVista.Core.Attributes.FormFieldAttribute(FieldType: LagoVista.Core.Attributes.FieldTypes.LinkButton));

            response.View["linkButton"].Command = new RelayCommand(HideLinkButton);

            frmEditPasswordLink.Label = "Edit Password";
            frmEditPasswordLink.Name = "editPassword";
            frmEditPasswordLink.Watermark = "-edit password-";
            frmEditPasswordLink.IsVisible = true;
            frmEditPasswordLink.Command = new RelayCommand(EditPasswordTap);

            response.View.Add("editPassword", frmEditPasswordLink);

            FormAdapter = new EditFormAdapter(this, response.View, this.ViewModelNavigation);

            FormAdapter.AddViewCell(nameof(Model1.TextField1));
            FormAdapter.AddViewCell(nameof(Model1.DropDownBox1));
            FormAdapter.AddViewCell(nameof(Model1.CheckBox1));
            FormAdapter.AddViewCell("editPassword");
            FormAdapter.AddViewCell(nameof(Model1.LinkButton));
            FormAdapter.AddViewCell(nameof(Model1.Password));
            FormAdapter.AddViewCell(nameof(Model1.MultiLine1));

            FormAdapter.AddChildList<ViewModel2>(nameof(Model1.Model2Litems), model1.Model2Litems);

            return base.InitAsync();
        }

        public void EditPasswordTap()
        {
            FormAdapter.HideView("EditPassword");
        }

        public void HideLinkButton()
        {
             FormAdapter.HideView(nameof(Model1.LinkButton));
        }

        EditFormAdapter _formAdapter;
        public EditFormAdapter FormAdapter
        {
            get { return _formAdapter; }
            private set { Set(ref _formAdapter, value); }
        }

    }

}
