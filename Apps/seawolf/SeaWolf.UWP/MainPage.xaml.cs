using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SeaWolf.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            var version = typeof(App).GetTypeInfo().Assembly.GetName().Version;

            var versionInfo = new VersionInfo()
            {
                Major = version.Major,
                Minor = version.Minor,
                Revision = version.Revision,
                Build = version.Build,
            };

            var app = new SeaWolf.App();
            app.SetVersionInfo(versionInfo);
            LoadApplication(app);


        }
    }
}
