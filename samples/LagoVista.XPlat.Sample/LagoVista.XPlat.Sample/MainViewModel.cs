using System.Threading.Tasks;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Models.UIMetaData;
using System.Collections.Generic;
using LagoVista.Core.Models;
using LagoVista.Core.Commanding;

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
            };
        }

        public override Task InitAsync()
        {
            var model1 = new Model1();
            model1.Model2Litems = new List<Model2>();
            var response = DetailResponse<Model1>.Create(model1);

            FormAdapter = new EditFormAdapter(this, response.View, this.ViewModelNavigation);

            FormAdapter.AddViewCell(nameof(Model1.TextField1));
            FormAdapter.AddViewCell(nameof(Model1.DropDownBox1));
            FormAdapter.AddViewCell(nameof(Model1.CheckBox1));
            FormAdapter.AddViewCell(nameof(Model1.MultiLine1));
            FormAdapter.AddViewCell(nameof(Model1.Password));

            FormAdapter.AddChildList<ViewModel2>(nameof(Model1.Model2Litems), model1.Model2Litems);

            return base.InitAsync();
        }

        EditFormAdapter _formAdapter;
        public EditFormAdapter FormAdapter
        {
            get { return _formAdapter; }
            private set { Set(ref _formAdapter, value); }
        }

    }

}
