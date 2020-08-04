using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.XPlat.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Sample
{
    public class MainViewModel : FormViewModelBase<Model1>
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
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<ChangePasswordViewModel>(this)),
                    Name = "Change Password",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<InviteUserViewModel>(this)),
                    Name = "Invite User",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<AcceptInviteViewModel>(this)),
                    Name = "Accept Invite",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<UserOrgsViewModel>(this)),
                    Name = "Change Orgs",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<OrgEditorViewModel>(this)),
                    Name = "Org Editor",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<FullPageViewModel>(this)),
                    Name = "Full Screen",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SettingsViewModel>(this)),
                    Name = "Settings View",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<NetworkingViewModel>(this)),
                    Name = "Networking View",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<WiFiNetworksViewModel>(this)),
                    Name = "WiFi Network View",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(StartListening),
                    Name = "Start COM7 Serial Port",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(StartSDPListener),
                    Name = "Start SSDP Listener",
                    FontIconKey = "fa-gear"
                },
                 new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<BTSerialViewModel>(this)),
                    Name = "BT Serial View",
                    FontIconKey = "fa-gear"
                },
            };
        }

        protected override string GetHelpLink()
        {
            return "http://support.nuviot.com/help.html#/simulator/index";
        }


        public void HandleMe(object item)
        {
            Debug.WriteLine(item.ToString());
        }

        public override Task InitAsync()
        {
            var model1 = new Model1();
            model1.TextField1 = "DEFAULT FIELD VALUE";
            model1.Model2Litems = new List<Model2>();
            model1.Model2Litems.Add(new Model2() { Id = "12337", Text = "A little bit of tex" });
            model1.Model2Litems.Add(new Model2() { Id = "13239", Text = "This is good" });
            model1.Model2Litems.Add(new Model2() { Id = "1234", Text = "Item A" });
            model1.Model2Litems.Add(new Model2() { Id = "1235", Text = "Item B" });
            model1.Model2Litems.Add(new Model2() { Id = "1237", Text = "Item C" });
            model1.Model2Litems.Add(new Model2() { Id = "1239", Text = "Item D" });

            var response = DetailResponse<Model1>.Create(model1);

            var frmEditPasswordLink = FormField.Create("EditPassword",
            new LagoVista.Core.Attributes.FormFieldAttribute(FieldType: LagoVista.Core.Attributes.FieldTypes.LinkButton),
            null);

            //    response.View["linkButton"].Command = new RelayCommand(HideLinkButton);


            frmEditPasswordLink.Label = "Edit Password";
            frmEditPasswordLink.Name = "editPassword";
            frmEditPasswordLink.Watermark = "-edit password-";
            frmEditPasswordLink.IsVisible = true;
            frmEditPasswordLink.Command = new RelayCommand(EditPasswordTap);

            response.View.Add("editPassword", frmEditPasswordLink);

            var formAdapter = new EditFormAdapter(model1, response.View, this.ViewModelNavigation);

            //response.View[nameof(Model1.MySecretField)].Command = new RelayCommand((arg) => HandleMe(arg));

            formAdapter.AddViewCell(nameof(Model1.TextField1));
            formAdapter.AddViewCell(nameof(Model1.DropDownBox1));
            formAdapter.AddViewCell(nameof(Model1.CheckBox1));
            formAdapter.AddViewCell("editPassword");
            formAdapter.AddViewCell(nameof(Model1.LinkButton));
            formAdapter.AddViewCell(nameof(Model1.MySecretField));
            formAdapter.AddViewCell(nameof(Model1.Password));
            formAdapter.AddViewCell(nameof(Model1.MultiLine1));

            formAdapter.AddChildList<ViewModel2>(nameof(Model1.Model2Litems), model1.Model2Litems);

            var field = nameof(Model1.MySecretField).ToJSONName();

            formAdapter.FormItems.Where(itm => itm.Name == field).First().Command = new RelayCommand((arg) => HandleMe(arg));
            formAdapter.FormItems.Where(itm => itm.Name == "checkBox1").First().IsVisible = false;

            ModelToView(model1, formAdapter);

            FormAdapter = formAdapter;

            return Task.CompletedTask;


            //            await base.InitAsync();
        }

        private async void StartSDPListener()
        {
            var config = new LagoVista.Core.Networking.Models.UPNPConfiguration()
            {
                DefaultPageHtml = "<html>HelloWorld</html>",
                DeviceType = "X_LagoVista_ISY_Kiosk_Device",
                FriendlyName = "ISY Remote Kiosk",
                Manufacture = "Software Logistics, LLC",
                ManufactureUrl = "www.TheWolfBytes.com",
                ModelDescription = "ISY Remote UI and SmartThings Bridge",
                ModelName = "ISYRemoteKiosk",
                ModelNumber = "SL001",
                ModelUrl = "www.TheWolfBytes.com",
                SerialNumber = "KSK001"
            };
            /*
            try
            {                
                await RestClient.MakeDiscoverableAsync(9301, config);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }*/
        }

        private async void StartListening()
        {
            var ports = await DeviceManager.GetSerialPortsAsync();
            var port = ports.Where(prt => prt.Name.Contains("COM3")).FirstOrDefault();
            port.BaudRate = 115200;
            port.Parity = false;
            port.DataBits = 8;

            var serialPort = DeviceManager.CreateSerialPort(port);
            await serialPort.OpenAsync();

            await Task.Run(async () =>
            {
                var _running = true;
                var buffer = new byte[128];
                //https://mavlink.io/en/guide/serialization.html
                //https://mavlink.io/en/protocol/overview.html
                //TODO: Move to a service class
                while (_running)
                {
                    var readCount = await serialPort.ReadAsync(buffer, 0, buffer.Length);
                    Debug.WriteLine("Bytes Read" + readCount);
                    for (var idx = 0; idx < readCount; ++idx)
                    {
                        if (buffer[idx] == 0xFD)
                        {
                            Debug.WriteLine("=====================\r\nSTART MVLINK 2");
                        }
                        else if (buffer[idx] == 0xFE)
                        {
                            Debug.WriteLine("=====================\r\nSTART MVLINK 1");
                        }
                        else
                        {
                            Debug.WriteLine($"{idx:000}.  {buffer[idx]:x2}");
                        }
                    }
                }

                Debug.WriteLine("Start next batch\r\r\r");
            });
        }

        public void EditPasswordTap()
        {
            FormAdapter.HideView("EditPassword");
        }

        public void HideLinkButton()
        {
            FormAdapter.HideView(nameof(Model1.CheckBox1));
            FormAdapter.HideView(nameof(Model1.LinkButton));
        }

        public override Task<InvokeResult> SaveRecordAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override string GetRequestUri()
        {
            throw new System.NotImplementedException();
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            throw new System.NotImplementedException();
        }

        EditFormAdapter _formAdapter;
        public EditFormAdapter FormAdapter
        {
            get { return _formAdapter; }
            private set { Set(ref _formAdapter, value); }
        }

    }

}
