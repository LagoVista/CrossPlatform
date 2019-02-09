using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.UIMetaData;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<FullPageViewModel>(this)),
                    Name = "Full SCreen",
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
            };
        }

        public async override Task InitAsync()
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

            await base.InitAsync();
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

            try
            {

                await _restClient.MakeDiscoverableAsync(9301, config);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void StartListening()
        {
            var ports = await DeviceManager.GetSerialPortsAsync();
            var port = ports.Where(prt => prt.Name.Contains("COM7")).FirstOrDefault();
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
