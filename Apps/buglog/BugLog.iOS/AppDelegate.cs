using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using LagoVista.Core.Models;
using LagoVista.XPlat.Core.Services;
using UIKit;

namespace SeaWolf.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

            LagoVista.XPlat.iOS.Startup.Init(app, SeaWolf.App.AppCenterId_iOS);

            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.FormsMaps.Init();
            DeviceInfo.Register();

            var formsApp = new App();

            var version = NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleVersion")].ToString();
            Console.WriteLine($"NSLog Version {version}");

            var versionParts = version.Split('.');
            var versionInfo = new VersionInfo();
            if (versionParts.Length != 4)
            {
                throw new Exception("Expecting CFBundleVersion to be a version consisting of four parts 1.0.218.1231 [Major].[Minor].[Build].[Revision]");
            }

            /* if this blows up our build version is borked...make sure all version numbers are intergers like 1.0.218.1231 */
            versionInfo.Major = Convert.ToInt32(versionParts[0]);
            versionInfo.Minor = Convert.ToInt32(versionParts[1]);
            versionInfo.Build = Convert.ToInt32(versionParts[2]);
            versionInfo.Revision = Convert.ToInt32(versionParts[3]);
            formsApp.SetVersionInfo(versionInfo);

            LoadApplication(formsApp);

            return base.FinishedLaunching(app, options);
        }
    }
}
