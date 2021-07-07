using System.Reflection;

namespace LagoVista.PlatformManager.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            var version = typeof(App).GetTypeInfo().Assembly.GetName().Version;

            var versionInfo = new LagoVista.Core.Models.VersionInfo()
            {
                Major = version.Major,
                Minor = version.Minor,
                Revision = version.Revision,
                Build = version.Build,
            };

            var app = new LagoVista.PlatformManager.App();
            app.SetVersionInfo(versionInfo);
            LoadApplication(app);

            var btn = new LagoVista.XPlat.Core.Button();
        }
    }
}
